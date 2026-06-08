using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryController : ControllerBase
    {
        private readonly FactoryService _factoryService;

        public FactoryController(FactoryService factoryService)
        {
            _factoryService = factoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FactoryDto>>>> GetAll()
        {
            var result = await _factoryService.GetAllAsync();
            return Ok(ApiResponse<List<FactoryDto>>.Ok(result));
        }

        [HttpGet("{factoryId}")]
        public async Task<ActionResult<ApiResponse<FactoryDto>>> GetById(string factoryId)
        {
            var result = await _factoryService.GetByIdAsync(factoryId);
            if (result == null)
            {
                return NotFound(ApiResponse<FactoryDto>.Fail("Factory not found"));
            }

            return Ok(ApiResponse<FactoryDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<FactoryDto>>> Create(FactoryCreateDto dto)
        {
            var result = await _factoryService.CreateAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<FactoryDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<FactoryDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{factoryId}")]
        public async Task<ActionResult<ApiResponse<FactoryDto>>> Update(string factoryId, FactoryUpdateDto dto)
        {
            var result = await _factoryService.UpdateAsync(factoryId, dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<FactoryDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<FactoryDto>.Ok(result.Data!, result.Message));
        }

        [HttpDelete("{factoryId}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(string factoryId)
        {
            var result = await _factoryService.DeleteAsync(factoryId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<string>.Fail(result.Message));
            }

            return Ok(ApiResponse<string>.Ok(result.Message, result.Message));
        }
    }
}
