using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null)
            {
                return Unauthorized(ApiResponse<string>.Fail("EmployeeId or Password is incorrect"));
            }

            return Ok(ApiResponse<string>.Ok(token));
        }
    }
}
