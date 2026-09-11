namespace Biblioteca.Application.Authentication;

public record RegistrationResult(string? UserId, IReadOnlyList<string> Errors)
{
    public bool Succeeded => UserId is not null;
}
