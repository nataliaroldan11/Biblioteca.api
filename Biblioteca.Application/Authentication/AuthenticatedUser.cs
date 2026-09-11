namespace Biblioteca.Application.Authentication;

// Datos propios de Application: IdentityUser no sale de Infrastructure.
public record AuthenticatedUser(string Id, string Email);
