using Biblioteca.Application.Authentication;

namespace Biblioteca.Application.UseCases.Authentication;

public class LoginUseCase
{
    private readonly IIdentityService _identity;
    private readonly ITokenService _tokens;

    public LoginUseCase(IIdentityService identity, ITokenService tokens)
    {
        _identity = identity;
        _tokens = tokens;
    }

    public async Task<AuthTokenResponse?> ExecuteAsync(LoginRequest request)
    {
        var user = await _identity.AuthenticateAsync(request.Email.Trim(), request.Password);
        if (user is null)
            return null;

        // Solo emitimos un token para la identidad verificada por el servicio.
        return _tokens.CreateToken(user);
    }
}
