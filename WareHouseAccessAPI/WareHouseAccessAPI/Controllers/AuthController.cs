using Microsoft.AspNetCore.Mvc;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Services;

namespace WarehouseAccessAPI.Controllers;

[ApiController]
[Route("WarehouseAccess/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [HttpPost("/api/login")]
    public async Task<ActionResult<Response<LoginUserProfileDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            if (result.Message == "UserId, FactoryId and Password are required")
            {
                return BadRequest(result);
            }
            if (result.Message == "Account is disabled")
            {
                return Unauthorized(result);
            }
            return NotFound(result);
        }

        return Ok(result);
    }
}
