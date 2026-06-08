using Microsoft.AspNetCore.Mvc;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Services;

namespace WarehouseAccessAPI.Controllers;

[ApiController]
[Route("WarehouseAccess/[controller]/[action]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<ActionResult<Response<LookupByCardResponseDto>>> LookupByCard([FromBody] LookupByCardRequestDto request)
    {
        var result = await _transactionService.LookupByCardAsync(request, HttpContext.RequestAborted);
        if (!result.Success)
        {
            if (result.Message == "CardNumber is required" || result.Message.Contains("valid number"))
            {
                return BadRequest(result);
            }
            if (result.Message == "Card not found")
            {
                return NotFound(result);
            }
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<TransactionDetailDto>>> CreateCheckIn([FromBody] CreateTransactionCheckInRequestDto request)
    {
        var result = await _transactionService.CreateCheckInAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("required"))
            {
                return BadRequest(result);
            }
            if (result.Message.Contains("already checked in"))
            {
                return Conflict(result);
            }
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<TransactionDetailDto>>> ConfirmCheckOut([FromBody] ConfirmCheckOutRequestDto request)
    {
        var result = await _transactionService.ConfirmCheckOutAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("required") || result.Message.Contains("Already checked out"))
            {
                return BadRequest(result);
            }
            if (result.Message == "Transaction not found")
            {
                return NotFound(result);
            }
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<TransactionDetailDto>>>> GetLiveMonitor([FromQuery] string? keyword, [FromQuery] int take = 200)
    {
        var result = await _transactionService.GetLiveMonitorAsync(keyword, take);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<TransactionDashboardStatsDto>>> GetDashboardStats()
    {
        var result = await _transactionService.GetDashboardStatsAsync();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<PagedResult<TransactionDetailDto>>>> GetHistory(
        [FromQuery] string? keyword, 
        [FromQuery] DateTime? fromDate, 
        [FromQuery] DateTime? toDate, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 15)
    {
        var result = await _transactionService.GetHistoryAsync(keyword, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> ExportHistoryExcel([FromQuery] string? keyword, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var excelBytes = await _transactionService.ExportHistoryExcelBytesAsync(keyword, fromDate, toDate);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"warehouse-access-{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
