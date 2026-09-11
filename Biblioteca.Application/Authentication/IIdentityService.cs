namespace Biblioteca.Application.Authentication;

public interface IIdentityService
{
    Task<RegistrationResult> RegisterAsync(string email, string password);

    // Devuelve null si las credenciales no son válidas o la cuenta está bloqueada.
    Task<AuthenticatedUser?> AuthenticateAsync(string email, string password);
}
