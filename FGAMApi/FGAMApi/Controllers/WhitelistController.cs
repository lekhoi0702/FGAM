using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhitelistController : ControllerBase
    {
        private readonly WhitelistService _whitelistService;

        public WhitelistController(WhitelistService whitelistService)
        {
            _whitelistService = whitelistService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WhitelistDto>>>> GetAll()
        {
            var result = await _whitelistService.GetAllAsync();
            return Ok(ApiResponse<List<WhitelistDto>>.Ok(result));
        }

        [HttpGet("{whitelistId:int}")]
        public async Task<ActionResult<ApiResponse<WhitelistDto>>> GetById(int whitelistId)
        {
            var result = await _whitelistService.GetByIdAsync(whitelistId);
            if (result == null)
            {
                return NotFound(ApiResponse<WhitelistDto>.Fail("Whitelist not found"));
            }

            return Ok(ApiResponse<WhitelistDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WhitelistDto>>> Create(WhitelistCreateDto dto)
        {
            var result = await _whitelistService.CreateAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<WhitelistDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<WhitelistDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{whitelistId:int}")]
        public async Task<ActionResult<ApiResponse<WhitelistDto>>> Update(int whitelistId, WhitelistUpdateDto dto)
        {
            var result = await _whitelistService.UpdateAsync(whitelistId, dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<WhitelistDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<WhitelistDto>.Ok(result.Data!, result.Message));
        }

        [HttpDelete("{whitelistId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int whitelistId)
        {
            var result = await _whitelistService.DeleteAsync(whitelistId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<string>.Fail(result.Message));
            }

            return Ok(ApiResponse<string>.Ok(result.Message, result.Message));
        }
    }
}
