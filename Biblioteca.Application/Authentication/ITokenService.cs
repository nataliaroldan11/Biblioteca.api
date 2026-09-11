namespace Biblioteca.Application.Authentication;

public interface ITokenService
{
    AuthTokenResponse CreateToken(AuthenticatedUser user);
}
