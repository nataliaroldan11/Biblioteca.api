using Biblioteca.Application.Authentication;
using Biblioteca.Application.UseCases.Authentication;
using Xunit;

namespace Biblioteca.Application.Tests;

public class AuthenticationUseCaseTests
{
    [Fact]
    public async Task Register_NormalizesEmailAndPreservesPassword()
    {
        var identity = new StubIdentityService();
        var useCase = new RegisterUserUseCase(identity);

        var result = await useCase.ExecuteAsync(new RegisterRequest
        {
            Email = "  lector@example.com  ",
            Password = " Password!2026 "
        });

        Assert.True(result.Succeeded);
        Assert.Equal("user-42", result.UserId);
        Assert.Empty(result.Errors);
        Assert.Equal("lector@example.com", identity.ReceivedEmail);
        Assert.Equal(" Password!2026 ", identity.ReceivedPassword);
    }

    [Fact]
    public async Task Register_RejectedByIdentity_ReturnsFailureWithErrors()
    {
        var identity = new StubIdentityService
        {
            Registration = new RegistrationResult(null, new[] { "Email already registered" })
        };
        var useCase = new RegisterUserUseCase(identity);

        var result = await useCase.ExecuteAsync(new RegisterRequest
        {
            Email = "lector@example.com", Password = "Password!2026"
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);
        Assert.Equal("Email already registered", Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Login_RejectedCredentials_DoesNotIssueToken()
    {
        var identity = new StubIdentityService { User = null };
        var tokens = new RecordingTokenService();
        var useCase = new LoginUseCase(identity, tokens);

        var result = await useCase.ExecuteAsync(new LoginRequest
        {
            Email = "lector@example.com", Password = "WrongPassword!2026"
        });

        Assert.Null(result);
        Assert.Equal(0, tokens.IssuedTokens);
        Assert.Equal("WrongPassword!2026", identity.ReceivedPassword);
    }

    [Fact]
    public async Task Login_ValidCredentials_IssuesTokenForAuthenticatedIdentity()
    {
        var identity = new StubIdentityService
        {
            User = new AuthenticatedUser("user-42", "Lector@example.com")
        };
        var tokens = new RecordingTokenService();
        var useCase = new LoginUseCase(identity, tokens);

        var result = await useCase.ExecuteAsync(new LoginRequest
        {
            Email = "  lector@example.com  ", Password = " Password!2026 "
        });

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 30, 0, TimeSpan.Zero), result.ExpiresAtUtc);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal("lector@example.com", identity.ReceivedEmail);
        Assert.Equal(" Password!2026 ", identity.ReceivedPassword);
        Assert.Equal(1, tokens.IssuedTokens);
        Assert.NotNull(tokens.ReceivedUser);
        Assert.Equal("user-42", tokens.ReceivedUser.Id);
        Assert.Equal("Lector@example.com", tokens.ReceivedUser.Email);
    }

    // Dobles de los límites externos: estas pruebas no necesitan Identity ni una base de datos.
    private sealed class StubIdentityService : IIdentityService
    {
        public RegistrationResult Registration { get; init; } = new("user-42", Array.Empty<string>());
        public AuthenticatedUser? User { get; init; }
        public string? ReceivedEmail { get; private set; }
        public string? ReceivedPassword { get; private set; }

        public Task<RegistrationResult> RegisterAsync(string email, string password)
        {
            ReceivedEmail = email;
            ReceivedPassword = password;
            return Task.FromResult(Registration);
        }

        public Task<AuthenticatedUser?> AuthenticateAsync(string email, string password)
        {
            ReceivedEmail = email;
            ReceivedPassword = password;
            return Task.FromResult(User);
        }
    }

    private sealed class RecordingTokenService : ITokenService
    {
        public int IssuedTokens { get; private set; }
        public AuthenticatedUser? ReceivedUser { get; private set; }

        public AuthTokenResponse CreateToken(AuthenticatedUser user)
        {
            IssuedTokens++;
            ReceivedUser = user;
            return new AuthTokenResponse("test-access-token",
                new DateTimeOffset(2030, 1, 1, 0, 30, 0, TimeSpan.Zero));
        }
    }
}
