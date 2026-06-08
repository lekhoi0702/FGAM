using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetAll()
        {
            var result = await _departmentService.GetAllAsync();
            return Ok(ApiResponse<List<DepartmentDto>>.Ok(result));
        }

        [HttpGet("{departmentId}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(string departmentId)
        {
            var result = await _departmentService.GetByIdAsync(departmentId);
            if (result == null)
            {
                return NotFound(ApiResponse<DepartmentDto>.Fail("Department not found"));
            }

            return Ok(ApiResponse<DepartmentDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create(DepartmentCreateDto dto)
        {
            var result = await _departmentService.CreateAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<DepartmentDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<DepartmentDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{departmentId}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(string departmentId, DepartmentUpdateDto dto)
        {
            var result = await _departmentService.UpdateAsync(departmentId, dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<DepartmentDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<DepartmentDto>.Ok(result.Data!, result.Message));
        }

        [HttpDelete("{departmentId}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(string departmentId)
        {
            var result = await _departmentService.DeleteAsync(departmentId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<string>.Fail(result.Message));
            }

            return Ok(ApiResponse<string>.Ok(result.Message, result.Message));
        }
    }
}
