using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class DepartmentService
    {
        private readonly FgamContext _context;

        public DepartmentService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<DepartmentDto> BuildQuery()
        {
            return _context.Departments
                .AsNoTracking()
                .Where(x => x.RecordStatus == 1)
                .Select(x => new DepartmentDto
                {
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.DepartmentName
                });
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            return await BuildQuery().ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(string departmentId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId);
        }

        public async Task<(bool Success, string Message, DepartmentDto? Data)> CreateAsync(DepartmentCreateDto dto)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(x => x.DepartmentId == dto.DepartmentId);

            if (departmentExists)
            {
                return (false, "DepartmentId already exists", null);
            }

            var now = DateTime.Now;
            var department = new Department
            {
                DepartmentId = dto.DepartmentId,
                DepartmentName = dto.DepartmentName,
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(department.DepartmentId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, DepartmentDto? Data)> UpdateAsync(string departmentId, DepartmentUpdateDto dto)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.RecordStatus == 1);

            if (department == null)
            {
                return (false, "Department not found", null);
            }

            department.DepartmentName = dto.DepartmentName;
            department.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(department.DepartmentId);
            return (true, "Updated successfully", data);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string departmentId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.RecordStatus == 1);

            if (department == null)
            {
                return (false, "Department not found");
            }

            department.RecordStatus = 0;
            department.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Deleted successfully");
        }
    }
}
