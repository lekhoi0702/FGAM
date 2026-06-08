using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Services;

using WarehouseAccessAPI.Attributes;

namespace WarehouseAccessAPI.Controllers;

[ApiController]
[Route("WarehouseAccess/[controller]/[action]")]
[MasterDataAuthorize]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<DepartmentItemDto>>>> GetDepartments()
    {
        var result = await _departmentService.GetDepartmentsAsync();
        return Ok(result);
    }

    [HttpGet]
    public IActionResult ExportDepartmentsTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DepartmentsTemplate");
        worksheet.Cell(1, 1).Value = "FactoryId";
        worksheet.Cell(1, 2).Value = "DepartmentId";
        worksheet.Cell(1, 3).Value = "DepartmentName";
        worksheet.Columns(1, 3).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"departments-template-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost]
    public async Task<ActionResult<Response<DepartmentItemDto>>> CreateDepartment([FromBody] DepartmentUpsertRequestDto request)
    {
        var result = await _departmentService.CreateDepartmentAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("required") || result.Message.Contains("invalid") || result.Message.Contains("not found"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<Response<DepartmentItemDto>>> UpdateDepartment([FromBody] DepartmentUpsertRequestDto request)
    {
        var result = await _departmentService.UpdateDepartmentAsync(request);
        if (!result.Success)
        {
            if (result.Message == "Department not found")
            {
                return NotFound(result);
            }
            if (result.Message.Contains("required"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult<Response<bool>>> DeleteDepartment([FromQuery] string? departmentId)
    {
        if (string.IsNullOrWhiteSpace(departmentId))
        {
            return BadRequest(new Response<bool>(false, false, "departmentId is required"));
        }

        var result = await _departmentService.DeleteDepartmentAsync(departmentId);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
