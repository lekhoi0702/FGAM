using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAll()
        {
            var result = await _employeeService.GetAllAsync();
            return Ok(ApiResponse<List<EmployeeDto>>.Ok(result));
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(string employeeId)
        {
            var result = await _employeeService.GetByIdAsync(employeeId);
            if (result == null)
            {
                return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found"));
            }

            return Ok(ApiResponse<EmployeeDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(EmployeeCreateDto dto)
        {
            var result = await _employeeService.CreateAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<EmployeeDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<EmployeeDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{employeeId}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(string employeeId, EmployeeUpdateDto dto)
        {
            var result = await _employeeService.UpdateAsync(employeeId, dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<EmployeeDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<EmployeeDto>.Ok(result.Data!, result.Message));
        }

        [HttpDelete("{employeeId}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(string employeeId)
        {
            var result = await _employeeService.DeleteAsync(employeeId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<string>.Fail(result.Message));
            }

            return Ok(ApiResponse<string>.Ok(result.Message, result.Message));
        }
    }
}
