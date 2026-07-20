using Game.Api.Contracts;
using Game.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<LoginResultDto>> LoginWithGoogle([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.LoginWithGoogleAsync(request.IdToken);

        if (result is null)
        {
            return Unauthorized(new { message = "Não foi possível validar o login com o Google. Tente novamente." });
        }

        return Ok(result);
    }
}
