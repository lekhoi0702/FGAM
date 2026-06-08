using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class EmployeeService
    {
        private readonly FgamContext _context;

        public EmployeeService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<EmployeeDto> BuildQuery()
        {
            return
                from e in _context.Employees.AsNoTracking()
                join f in _context.Factories.AsNoTracking()
                    on e.FactoryId equals f.FactoryId
                join d in _context.Departments.AsNoTracking()
                    on e.DepartmentId equals d.DepartmentId
                where e.RecordStatus == 1
                select new EmployeeDto
                {
                    FactoryId = e.FactoryId,
                    FactoryName = f.FactoryName,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    CardNumber = e.CardNumber
                };
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            return await BuildQuery().ToListAsync();
        }

        public async Task<EmployeeDto?> GetByIdAsync(string employeeId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }

        public async Task<(bool Success, string Message, EmployeeDto? Data)> CreateAsync(EmployeeCreateDto dto)
        {
            var employeeExists = await _context.Employees
                .AnyAsync(x => x.EmployeeId == dto.EmployeeId);

            if (employeeExists)
            {
                return (false, "EmployeeId already exists", null);
            }

            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);

            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var departmentExists = await _context.Departments
                .AnyAsync(x => x.DepartmentId == dto.DepartmentId);

            if (!departmentExists)
            {
                return (false, "DepartmentId is invalid", null);
            }

            var now = DateTime.Now;
            var employee = new Employee
            {
                FactoryId = dto.FactoryId,
                DepartmentId = dto.DepartmentId,
                EmployeeId = dto.EmployeeId,
                EmployeeName = dto.EmployeeName,
                CardNumber = dto.CardNumber,
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(employee.EmployeeId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, EmployeeDto? Data)> UpdateAsync(string employeeId, EmployeeUpdateDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.RecordStatus == 1);

            if (employee == null)
            {
                return (false, "Employee not found", null);
            }

            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);

            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var departmentExists = await _context.Departments
                .AnyAsync(x => x.DepartmentId == dto.DepartmentId);

            if (!departmentExists)
            {
                return (false, "DepartmentId is invalid", null);
            }

            employee.FactoryId = dto.FactoryId;
            employee.DepartmentId = dto.DepartmentId;
            employee.EmployeeName = dto.EmployeeName;
            employee.CardNumber = dto.CardNumber;
            employee.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(employee.EmployeeId);
            return (true, "Updated successfully", data);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.RecordStatus == 1);

            if (employee == null)
            {
                return (false, "Employee not found");
            }

            employee.RecordStatus = 0;
            employee.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Deleted successfully");
        }
    }
}
