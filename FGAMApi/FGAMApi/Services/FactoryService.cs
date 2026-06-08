using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class FactoryService
    {
        private readonly FgamContext _context;

        public FactoryService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<FactoryDto> BuildQuery()
        {
            return _context.Factories
                .AsNoTracking()
                .Where(x => x.RecordStatus == 1)
                .Select(x => new FactoryDto
                {
                    FactoryId = x.FactoryId,
                    FactoryName = x.FactoryName
                });
        }

        public async Task<List<FactoryDto>> GetAllAsync()
        {
            return await BuildQuery().ToListAsync();
        }

        public async Task<FactoryDto?> GetByIdAsync(string factoryId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.FactoryId == factoryId);
        }

        public async Task<(bool Success, string Message, FactoryDto? Data)> CreateAsync(FactoryCreateDto dto)
        {
            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);

            if (factoryExists)
            {
                return (false, "FactoryId already exists", null);
            }

            var now = DateTime.Now;
            var factory = new Factory
            {
                FactoryId = dto.FactoryId,
                FactoryName = dto.FactoryName,
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now
            };

            _context.Factories.Add(factory);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(factory.FactoryId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, FactoryDto? Data)> UpdateAsync(string factoryId, FactoryUpdateDto dto)
        {
            var factory = await _context.Factories
                .FirstOrDefaultAsync(x => x.FactoryId == factoryId && x.RecordStatus == 1);

            if (factory == null)
            {
                return (false, "Factory not found", null);
            }

            factory.FactoryName = dto.FactoryName;
            factory.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(factory.FactoryId);
            return (true, "Updated successfully", data);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string factoryId)
        {
            var factory = await _context.Factories
                .FirstOrDefaultAsync(x => x.FactoryId == factoryId && x.RecordStatus == 1);

            if (factory == null)
            {
                return (false, "Factory not found");
            }

            factory.RecordStatus = 0;
            factory.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Deleted successfully");
        }
    }
}
