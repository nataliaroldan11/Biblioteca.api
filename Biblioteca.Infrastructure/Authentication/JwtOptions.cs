using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "Biblioteca.Api";

    [Required]
    public string Audience { get; set; } = "Biblioteca.Client";

    [Range(1, 120)]
    public int ExpirationMinutes { get; set; } = 30;
}
