namespace Biblioteca.Application.Authentication;

// Application define el contrato; Infrastructure implementa Identity y JWT.
public interface IAuthService
{
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);
    Task<AuthTokenResponse?> LoginAsync(LoginRequest request);
}
