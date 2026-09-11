using Biblioteca.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
[RequestSizeLimit(16384)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request);
        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["registration"] = result.Errors.ToArray() })
            {
                Title = "No se pudo registrar el usuario.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return StatusCode(StatusCodes.Status201Created, new { id = result.UserId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await _auth.LoginAsync(request);
        if (token is null)
        {
            // Misma respuesta para usuario desconocido, contraseña incorrecta o bloqueo temporal.
            return Problem(title: "No se pudo iniciar sesión.",
                detail: "Credenciales incorrectas o cuenta bloqueada temporalmente.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(token);
    }
}
