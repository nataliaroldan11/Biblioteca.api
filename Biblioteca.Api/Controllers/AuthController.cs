using Biblioteca.Application.Authentication;
using Biblioteca.Application.UseCases.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
[RequestSizeLimit(16384)]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _register;
    private readonly LoginUseCase _login;

    public AuthController(RegisterUserUseCase register, LoginUseCase login)
    {
        _register = register;
        _login = login;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _register.ExecuteAsync(request);
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
        var token = await _login.ExecuteAsync(request);
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
