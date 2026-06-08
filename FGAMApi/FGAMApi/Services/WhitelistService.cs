using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class WhitelistService
    {
        private readonly FgamContext _context;

        public WhitelistService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<WhitelistDto> BuildQuery()
        {
            return
                from w in _context.Whitelists.AsNoTracking()
                join f in _context.Factories.AsNoTracking()
                    on w.FactoryId equals f.FactoryId
                join e in _context.Employees.AsNoTracking()
                    on w.EmployeeId equals e.EmployeeId
                where w.RecordStatus == 1
                select new WhitelistDto
                {
                    WhitelistId = w.WhitelistId,
                    FactoryId = w.FactoryId,
                    FactoryName = f.FactoryName,
                    EmployeeId = w.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    Avatar = w.Avatar
                };
        }

        public async Task<List<WhitelistDto>> GetAllAsync()
        {
            return await BuildQuery().ToListAsync();
        }

        public async Task<WhitelistDto?> GetByIdAsync(int whitelistId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.WhitelistId == whitelistId);
        }

        public async Task<(bool Success, string Message, WhitelistDto? Data)> CreateAsync(WhitelistCreateDto dto)
        {
            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);
            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var employeeExists = await _context.Employees
                .AnyAsync(x => x.EmployeeId == dto.EmployeeId);
            if (!employeeExists)
            {
                return (false, "EmployeeId is invalid", null);
            }

            var duplicate = await _context.Whitelists
                .AnyAsync(x => x.FactoryId == dto.FactoryId && x.EmployeeId == dto.EmployeeId && x.RecordStatus == 1);
            if (duplicate)
            {
                return (false, "Whitelist already exists", null);
            }

            var now = DateTime.Now;
            var whitelist = new Whitelist
            {
                FactoryId = dto.FactoryId,
                EmployeeId = dto.EmployeeId,
                Avatar = dto.Avatar,
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now
            };

            _context.Whitelists.Add(whitelist);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(whitelist.WhitelistId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, WhitelistDto? Data)> UpdateAsync(int whitelistId, WhitelistUpdateDto dto)
        {
            var whitelist = await _context.Whitelists
                .FirstOrDefaultAsync(x => x.WhitelistId == whitelistId && x.RecordStatus == 1);

            if (whitelist == null)
            {
                return (false, "Whitelist not found", null);
            }

            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);
            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var employeeExists = await _context.Employees
                .AnyAsync(x => x.EmployeeId == dto.EmployeeId);
            if (!employeeExists)
            {
                return (false, "EmployeeId is invalid", null);
            }

            var duplicate = await _context.Whitelists
                .AnyAsync(x => x.WhitelistId != whitelistId && x.FactoryId == dto.FactoryId && x.EmployeeId == dto.EmployeeId && x.RecordStatus == 1);
            if (duplicate)
            {
                return (false, "Whitelist already exists", null);
            }

            whitelist.FactoryId = dto.FactoryId;
            whitelist.EmployeeId = dto.EmployeeId;
            whitelist.Avatar = dto.Avatar;
            whitelist.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(whitelist.WhitelistId);
            return (true, "Updated successfully", data);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int whitelistId)
        {
            var whitelist = await _context.Whitelists
                .FirstOrDefaultAsync(x => x.WhitelistId == whitelistId && x.RecordStatus == 1);

            if (whitelist == null)
            {
                return (false, "Whitelist not found");
            }

            whitelist.RecordStatus = 0;
            whitelist.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Deleted successfully");
        }
    }
}
