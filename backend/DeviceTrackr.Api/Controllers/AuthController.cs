using DeviceTrackr.Api.Dtos;
using DeviceTrackr.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceTrackr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequestDto request)
    {
        var result = auth.Register(request);
        if (!result.Ok || result.Data is null)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        var result = auth.Login(request);
        if (!result.Ok || result.Data is null)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Data);
    }
}
