using BCrypt.Net;
using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class AccountService
    {
        private readonly FgamContext _context;

        public AccountService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<AccountDto> BuildQuery()
        {
            return
                from a in _context.Accounts.AsNoTracking()
                join f in _context.Factories.AsNoTracking()
                    on a.FactoryId equals f.FactoryId
                join e in _context.Employees.AsNoTracking()
                    on a.EmployeeId equals e.EmployeeId
                where a.RecordStatus == 1
                select new AccountDto
                {
                    AccountId = a.AccountId,
                    FactoryId = a.FactoryId,
                    FactoryName = f.FactoryName,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = e.EmployeeName
                };
        }

        public async Task<List<AccountDto>> GetAllAsync()
        {
            return await BuildQuery()
                .OrderByDescending(x => x.AccountId)
                .ToListAsync();
        }

        public async Task<AccountDto?> GetByIdAsync(int accountId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task<(bool Success, string Message, AccountDto? Data)> CreateAsync(AccountCreateDto dto)
        {
            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);
            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeId == dto.EmployeeId && x.FactoryId == dto.FactoryId);
            if (employee == null)
            {
                return (false, "EmployeeId is invalid", null);
            }

            var duplicate = await _context.Accounts
                .AnyAsync(x => x.FactoryId == dto.FactoryId && x.EmployeeId == dto.EmployeeId);
            if (duplicate)
            {
                return (false, "Account already exists", null);
            }

            var now = DateTime.Now;
            var account = new Account
            {
                FactoryId = dto.FactoryId,
                EmployeeId = dto.EmployeeId,
                Passwrd = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(account.AccountId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, AccountDto? Data)> UpdateAsync(int accountId, AccountUpdateDto dto)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.RecordStatus == 1);

            if (account == null)
            {
                return (false, "Account not found", null);
            }

            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);
            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeId == dto.EmployeeId && x.FactoryId == dto.FactoryId);
            if (employee == null)
            {
                return (false, "EmployeeId is invalid", null);
            }

            var duplicate = await _context.Accounts
                .AnyAsync(x => x.AccountId != accountId && x.FactoryId == dto.FactoryId && x.EmployeeId == dto.EmployeeId);
            if (duplicate)
            {
                return (false, "Account already exists", null);
            }

            account.FactoryId = dto.FactoryId;
            account.EmployeeId = dto.EmployeeId;
            account.Passwrd = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            account.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(account.AccountId);
            return (true, "Updated successfully", data);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int accountId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.RecordStatus == 1);

            if (account == null)
            {
                return (false, "Account not found");
            }

            account.RecordStatus = 0;
            account.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Deleted successfully");
        }
    }
}
