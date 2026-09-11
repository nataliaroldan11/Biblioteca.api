using Biblioteca.Application.Authentication;

namespace Biblioteca.Application.UseCases.Authentication;

public class RegisterUserUseCase
{
    private readonly IIdentityService _identity;

    public RegisterUserUseCase(IIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<RegistrationResult> ExecuteAsync(RegisterRequest request)
    {
        // Normalizamos el correo, pero preservamos la contraseña tal como se recibió.
        var email = request.Email.Trim();
        return await _identity.RegisterAsync(email, request.Password);
    }
}
