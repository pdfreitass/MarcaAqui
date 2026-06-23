using MarcaAqui.Api.DTOs;
using MarcaAqui.Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace MarcaAqui.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { erro = "Dados inválidos.", detalhes = ModelState });

        try
        {
            var tokenDto = await _authService.RegistrarAsync(dto);
            return Created(string.Empty, tokenDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { erro = "Dados inválidos.", detalhes = ModelState });

        try
        {
            var tokenDto = await _authService.LoginAsync(dto);
            return Ok(tokenDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { erro = ex.Message });
        }
    }
}
