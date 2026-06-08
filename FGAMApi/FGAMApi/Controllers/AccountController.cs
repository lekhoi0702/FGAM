using FGAMApi.Common;
using FGAMApi.DTOs;
using FGAMApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FGAMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AccountDto>>>> GetAll()
        {
            var result = await _accountService.GetAllAsync();
            return Ok(ApiResponse<List<AccountDto>>.Ok(result));
        }

        [HttpGet("{accountId:int}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetById(int accountId)
        {
            var result = await _accountService.GetByIdAsync(accountId);
            if (result == null)
            {
                return NotFound(ApiResponse<AccountDto>.Fail("Account not found"));
            }

            return Ok(ApiResponse<AccountDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AccountDto>>> Create(AccountCreateDto dto)
        {
            var result = await _accountService.CreateAsync(dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<AccountDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<AccountDto>.Ok(result.Data!, result.Message));
        }

        [HttpPut("{accountId:int}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> Update(int accountId, AccountUpdateDto dto)
        {
            var result = await _accountService.UpdateAsync(accountId, dto);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<AccountDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<AccountDto>.Ok(result.Data!, result.Message));
        }

        [HttpDelete("{accountId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int accountId)
        {
            var result = await _accountService.DeleteAsync(accountId);
            if (!result.Success)
            {
                return NotFound(ApiResponse<string>.Fail(result.Message));
            }

            return Ok(ApiResponse<string>.Ok(result.Message, result.Message));
        }
    }
}
