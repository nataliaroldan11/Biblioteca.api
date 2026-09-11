using Biblioteca.Application.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Infrastructure.Authentication;

public class IdentityAuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly JwtTokenService _tokens;

    public IdentityAuthService(UserManager<IdentityUser> users, JwtTokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<RegistrationResult> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email };

        // Identity calcula el hash y guarda el usuario. Nunca guardamos Password.
        var result = await _users.CreateAsync(user, request.Password);

        return result.Succeeded
            ? new RegistrationResult(user.Id, Array.Empty<string>())
            : new RegistrationResult(null, result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<AuthTokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || await _users.IsLockedOutAsync(user))
            return null;

        if (!await _users.CheckPasswordAsync(user, request.Password))
        {
            await _users.AccessFailedAsync(user);
            return null;
        }

        await _users.ResetAccessFailedCountAsync(user);
        return _tokens.CreateToken(user);
    }
}
