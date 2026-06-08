using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetAll()
        {
            var result = await _transactionService.GetAllAsync();
            return Ok(ApiResponse<List<TransactionDto>>.Ok(result));
        }

        [HttpGet("{transactionId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> GetById(long transactionId)
        {
            var result = await _transactionService.GetByIdAsync(transactionId);
            if (result == null)
            {
                return NotFound(ApiResponse<TransactionDto>.Fail("Transaction not found"));
            }

            return Ok(ApiResponse<TransactionDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> CheckIn(TransactionCreateDto dto)
        {
            var result = await _transactionService.CheckInAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<TransactionDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<TransactionDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{transactionId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> CheckOut(long transactionId)
        {
            var result = await _transactionService.CheckOutAsync(transactionId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<TransactionDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<TransactionDto>.Ok(result.Data!, result.Message));
        }
    }
}
