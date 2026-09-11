using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Biblioteca.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Biblioteca.Api.Tests;

public class AuthenticationTests
{
    private const string Email = "lector@example.test";
    private const string Password = "Biblioteca-Test!2026";
    private static object BookData(string title = "Libro de prueba") =>
        new { title, author = "Autor", isbn = "9780307474728", publicationYear = 1967 };

    [Fact]
    public async Task ReadEndpoints_ArePublic()
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/books")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/books/0")).StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/books")]
    [InlineData("PUT", "/api/books/1")]
    [InlineData("DELETE", "/api/books/1")]
    public async Task WriteEndpoints_WithoutToken_Return401(string method, string path)
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method != "DELETE") request.Content = JsonContent.Create(BookData());
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_StoresHashAndRejectsDuplicateEmail()
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        var response = await Register(client);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.SingleAsync();
        Assert.Equal(Email, user.Email);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.DoesNotContain("password", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var duplicate = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "LECTOR@example.test", password = Password });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Theory]
    [InlineData("no-es-un-correo", "Biblioteca-Test!2026")]
    [InlineData("lector@example.test", "abc")]
    public async Task Register_RejectsInvalidInput(string email, string password)
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.ToListAsync());
    }

    [Fact]
    public async Task Login_RejectsWrongPasswordAndUnknownUser()
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        Assert.Equal(HttpStatusCode.Created, (await Register(client)).StatusCode);
        var wrong = await client.PostAsJsonAsync("/api/auth/login",
            new { email = Email, password = "Incorrecta!2026" });
        var unknown = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "nadie@example.test", password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
    }

    [Fact]
    public async Task Login_ValidToken_AllowsCreateUpdateAndDelete()
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        Assert.Equal(HttpStatusCode.Created, (await Register(client)).StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var session = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = session.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal("Bearer", session.GetProperty("tokenType").GetString());
        var expires = session.GetProperty("expiresAtUtc").GetDateTimeOffset();
        Assert.InRange(expires - DateTimeOffset.UtcNow, TimeSpan.FromMinutes(28), TimeSpan.FromMinutes(31));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("HS256", jwt.Header.Alg);
        Assert.Equal("Biblioteca.Tests", jwt.Issuer);
        Assert.Contains("Biblioteca.Tests.Client", jwt.Audiences);
        Assert.False(string.IsNullOrWhiteSpace(jwt.Subject));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var created = await client.PostAsJsonAsync("/api/books", BookData());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/books/{id}", BookData("Actualizado"))).StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        var read = await client.GetFromJsonAsync<JsonElement>($"/api/books/{id}");
        Assert.Equal("Actualizado", read.GetProperty("title").GetString());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/books/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/books/{id}")).StatusCode);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    public async Task InvalidJwt_CannotModifyBooks(string defect)
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        var token = new JwtSecurityToken(
            issuer: defect == "issuer" ? "Otro.Emisor" : "Biblioteca.Tests",
            audience: defect == "audience" ? "Otro.Cliente" : "Biblioteca.Tests.Client",
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "test-user") },
            notBefore: DateTime.UtcNow.AddHours(-1),
            expires: defect == "expired" ? DateTime.UtcNow.AddMinutes(-10) : DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                defect == "signature" ? "AnotherKeyThatMustNeverBeAccepted1234567890" : LibraryApiFactory.TestKey)),
                SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/books", BookData())).StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Books.ToListAsync());
    }

    [Fact]
    public async Task RepeatedWrongPasswords_TemporarilyBlockLogin()
    {
        using var factory = new LibraryApiFactory();
        using var client = factory.CreateLibraryClient();
        Assert.Equal(HttpStatusCode.Created, (await Register(client)).StatusCode);
        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login",
                new { email = Email, password = "Incorrecta!2026" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login",
            new { email = Email, password = Password })).StatusCode);
    }

    private static Task<HttpResponseMessage> Register(HttpClient client) =>
        client.PostAsJsonAsync("/api/auth/register", new { email = Email, password = Password });
}
