using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Services;

using WarehouseAccessAPI.Attributes;

namespace WarehouseAccessAPI.Controllers;

[ApiController]
[Route("WarehouseAccess/[controller]/[action]")]
[MasterDataAuthorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public IActionResult ExportUsersTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("EmployeesTemplate");
        worksheet.Cell(1, 1).Value = "EmployeeId";
        worksheet.Cell(1, 2).Value = "CardNumber";
        worksheet.Cell(1, 3).Value = "EmployeeName";
        worksheet.Cell(1, 4).Value = "DepartmentId";
        worksheet.Cell(1, 5).Value = "FactoryId";
        worksheet.Columns(1, 5).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"employees-template-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<EmployeeListItemDto>>>> GetUsers()
    {
        var result = await _employeeService.GetEmployeesAsync();
        return Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult<Response<EmployeeListItemDto>>> CreateUser([FromBody] CreateEmployeeRequestDto request)
    {
        var result = await _employeeService.CreateEmployeeAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("required") || result.Message.Contains("invalid") || result.Message.Contains("valid number"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<Response<EmployeeListItemDto>>> UpdateUser([FromBody] UpdateEmployeeRequestDto request)
    {
        var result = await _employeeService.UpdateEmployeeAsync(request);
        if (!result.Success)
        {
            if (result.Message == "Employee not found")
            {
                return NotFound(result);
            }
            if (result.Message.Contains("required") || result.Message.Contains("invalid") || result.Message.Contains("valid number"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult<Response<bool>>> DeleteUser([FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new Response<bool>(false, false, "EmployeeId is required"));
        }

        var result = await _employeeService.DeleteEmployeeAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<bool>>> AddWhitelistUser([FromBody] AddWhitelistRequestDto request)
    {
        var result = await _employeeService.AddWhitelistUserAsync(request);
        if (!result.Success)
        {
            if (result.Message == "Employee not found")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<WhitelistUserDto>>>> GetWhitelistUsers()
    {
        var result = await _employeeService.GetWhitelistUsersAsync();
        return Ok(result);
    }

    [HttpGet]
    public IActionResult ExportWhitelistTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("WhitelistTemplate");
        worksheet.Cell(1, 1).Value = "UserId";
        worksheet.Cell(1, 2).Value = "Avatar";
        worksheet.Columns(1, 2).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"whitelist-template-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost]
    public async Task<ActionResult<Response<bool>>> RemoveWhitelistUser([FromBody] AddWhitelistRequestDto request)
    {
        var result = await _employeeService.RemoveWhitelistUserAsync(request);
        return Ok(result);
    }
}
