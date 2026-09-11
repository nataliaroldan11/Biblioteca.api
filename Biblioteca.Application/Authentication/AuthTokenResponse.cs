namespace Biblioteca.Application.Authentication;

public record AuthTokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string TokenType = "Bearer");
