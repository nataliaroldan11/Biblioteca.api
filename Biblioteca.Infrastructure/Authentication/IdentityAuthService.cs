using Biblioteca.Application.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Infrastructure.Authentication;

// Adaptador de Identity: gestiona usuarios y credenciales; no emite tokens.
public class IdentityAuthService : IIdentityService
{
    private readonly UserManager<IdentityUser> _users;

    public IdentityAuthService(UserManager<IdentityUser> users)
    {
        _users = users;
    }

    public async Task<RegistrationResult> RegisterAsync(string email, string password)
    {
        var user = new IdentityUser { UserName = email, Email = email };

        // Identity calcula el hash y guarda el usuario. Nunca guardamos Password.
        var result = await _users.CreateAsync(user, password);

        return result.Succeeded
            ? new RegistrationResult(user.Id, Array.Empty<string>())
            : new RegistrationResult(null, result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null || await _users.IsLockedOutAsync(user))
            return null;

        if (!await _users.CheckPasswordAsync(user, password))
        {
            await _users.AccessFailedAsync(user);
            return null;
        }

        await _users.ResetAccessFailedCountAsync(user);
        return new AuthenticatedUser(user.Id, user.Email ?? string.Empty);
    }
}
