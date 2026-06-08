using Microsoft.EntityFrameworkCore;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Services;

public class EmployeeService
{
    private readonly FgamContext _db;
    private const int ActiveRecordStatus = 1;

    public EmployeeService(FgamContext db)
    {
        _db = db;
    }

    public async Task<Response<List<EmployeeListItemDto>>> GetEmployeesAsync()
    {
        var items = await _db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Factory)
            .OrderBy(e => e.EmployeeId)
            .Select(e => new EmployeeListItemDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.DepartmentName,
                FactoryId = e.FactoryId,
                FactoryName = e.Factory.FactoryName,
                CardNumber = e.CardNumber.HasValue ? e.CardNumber.Value.ToString() : string.Empty,
                RecordStatus = e.RecordStatus,
                IsWhiteList = e.Whitelists.Any(w => w.RecordStatus == 1),
                CreateId = e.CreateId,
                CreateDate = e.CreateDate,
                UpdateId = e.UpdateId,
                UpdateDate = e.UpdateDate
            })
            .ToListAsync();

        return new Response<List<EmployeeListItemDto>>(true, items, "Success");
    }

    public async Task<Response<EmployeeListItemDto>> CreateEmployeeAsync(CreateEmployeeRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<EmployeeListItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.EmployeeName)
            || string.IsNullOrWhiteSpace(request.DepartmentId) || string.IsNullOrWhiteSpace(request.FactoryId))
        {
            return new Response<EmployeeListItemDto>(false, null, "EmployeeId, EmployeeName, DepartmentId and FactoryId are required");
        }

        var employeeId = request.EmployeeId.Trim();
        var factoryId = request.FactoryId.Trim();
        var departmentId = request.DepartmentId.Trim();

        long? cardNumber = null;
        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            if (long.TryParse(request.CardNumber.Trim(), out long parsedCard))
            {
                cardNumber = parsedCard;
            }
            else
            {
                return new Response<EmployeeListItemDto>(false, null, "CardNumber must be a valid number");
            }
        }

        if (await _db.Employees.AsNoTracking().AnyAsync(x => x.EmployeeId == employeeId))
        {
            return new Response<EmployeeListItemDto>(false, null, "Employee ID already exists");
        }

        if (cardNumber.HasValue && await _db.Employees.AsNoTracking().AnyAsync(x => x.CardNumber == cardNumber))
        {
            return new Response<EmployeeListItemDto>(false, null, "CardNumber already exists");
        }

        if (!await _db.Departments.AsNoTracking().AnyAsync(x => x.DepartmentId == departmentId))
        {
            return new Response<EmployeeListItemDto>(false, null, "DepartmentId is invalid");
        }

        if (!await _db.Factories.AsNoTracking().AnyAsync(x => x.FactoryId == factoryId))
        {
            return new Response<EmployeeListItemDto>(false, null, "FactoryId is invalid");
        }

        var now = DateTime.Now;
        var loginUserId = request.LoginUserId.Trim();

        var employee = new Employee
        {
            EmployeeId = employeeId,
            FactoryId = factoryId,
            DepartmentId = departmentId,
            EmployeeName = request.EmployeeName.Trim(),
            CardNumber = cardNumber,
            RecordStatus = ActiveRecordStatus,
            CreateId = loginUserId,
            CreateDate = now,
            UpdateId = loginUserId,
            UpdateDate = now
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var dto = await ToEmployeeListItemAsync(employeeId);
        return new Response<EmployeeListItemDto>(true, dto, "Success");
    }

    public async Task<Response<EmployeeListItemDto>> UpdateEmployeeAsync(UpdateEmployeeRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<EmployeeListItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.EmployeeName)
            || string.IsNullOrWhiteSpace(request.DepartmentId) || string.IsNullOrWhiteSpace(request.FactoryId))
        {
            return new Response<EmployeeListItemDto>(false, null, "EmployeeId, EmployeeName, DepartmentId and FactoryId are required");
        }

        var employeeId = request.EmployeeId.Trim();
        var entity = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

        if (entity is null)
        {
            return new Response<EmployeeListItemDto>(false, null, "Employee not found");
        }

        var factoryId = request.FactoryId.Trim();
        var departmentId = request.DepartmentId.Trim();

        long? cardNumber = null;
        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            if (long.TryParse(request.CardNumber.Trim(), out long parsedCard))
            {
                cardNumber = parsedCard;
            }
            else
            {
                return new Response<EmployeeListItemDto>(false, null, "CardNumber must be a valid number");
            }
        }

        if (cardNumber.HasValue && await _db.Employees.AsNoTracking().AnyAsync(x => x.EmployeeId != employeeId && x.CardNumber == cardNumber))
        {
            return new Response<EmployeeListItemDto>(false, null, "CardNumber already exists");
        }

        if (!await _db.Departments.AsNoTracking().AnyAsync(x => x.DepartmentId == departmentId))
        {
            return new Response<EmployeeListItemDto>(false, null, "DepartmentId is invalid");
        }

        if (!await _db.Factories.AsNoTracking().AnyAsync(x => x.FactoryId == factoryId))
        {
            return new Response<EmployeeListItemDto>(false, null, "FactoryId is invalid");
        }

        var loginUserId = request.LoginUserId.Trim();
        var now = DateTime.Now;

        entity.EmployeeName = request.EmployeeName.Trim();
        entity.DepartmentId = departmentId;
        entity.FactoryId = factoryId;
        entity.CardNumber = cardNumber;
        entity.UpdateId = loginUserId;
        entity.UpdateDate = now;

        await _db.SaveChangesAsync();
        var dto = await ToEmployeeListItemAsync(employeeId);
        return new Response<EmployeeListItemDto>(true, dto, "Success");
    }

    public async Task<Response<bool>> DeleteEmployeeAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return new Response<bool>(false, false, "EmployeeId is required");
        }

        var entity = await _db.Employees
            .Include(e => e.Accounts)
            .Include(e => e.Whitelists)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Trim());

        if (entity is null)
        {
            return new Response<bool>(false, false, "Employee not found");
        }

        _db.Accounts.RemoveRange(entity.Accounts);
        _db.Whitelists.RemoveRange(entity.Whitelists);
        _db.Employees.Remove(entity);
        
        await _db.SaveChangesAsync();
        return new Response<bool>(true, true, "Success");
    }

    public async Task<Response<bool>> AddWhitelistUserAsync(AddWhitelistRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<bool>(false, false, "UserId and loginUserId are required");
        }

        var employeeId = request.UserId.Trim();
        var employee = await _db.Employees.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
        if (employee is null)
        {
            return new Response<bool>(false, false, "Employee not found");
        }

        var normalizedAvatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : NormalizePhoto(request.Avatar);
        var existingWhitelist = await _db.Whitelists.FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.FactoryId == employee.FactoryId);
        var now = DateTime.Now;
        var loginUserId = request.LoginUserId.Trim();

        if (existingWhitelist is not null)
        {
            existingWhitelist.Avatar = normalizedAvatar;
            existingWhitelist.RecordStatus = ActiveRecordStatus;
            existingWhitelist.UpdateDate = now;
            existingWhitelist.UpdateId = loginUserId;
        }
        else
        {
            _db.Whitelists.Add(new Whitelist
            {
                FactoryId = employee.FactoryId,
                EmployeeId = employeeId,
                Avatar = normalizedAvatar,
                RecordStatus = ActiveRecordStatus,
                CreateDate = now,
                CreateId = loginUserId,
                UpdateDate = now,
                UpdateId = loginUserId
            });
        }

        await _db.SaveChangesAsync();
        return new Response<bool>(true, true, "User added to whitelist successfully");
    }

    public async Task<Response<List<WhitelistUserDto>>> GetWhitelistUsersAsync()
    {
        var items = await _db.Whitelists
            .AsNoTracking()
            .Include(w => w.Employee)
                .ThenInclude(e => e.Department)
            .Where(w => w.RecordStatus == ActiveRecordStatus)
            .OrderByDescending(w => w.UpdateDate)
            .Select(w => new WhitelistUserDto
            {
                UserId = w.EmployeeId,
                FullName = w.Employee.EmployeeName,
                DepartmentId = w.Employee.DepartmentId,
                DepartmentName = w.Employee.Department.DepartmentName,
                Avatar = w.Avatar
            })
            .ToListAsync();

        return new Response<List<WhitelistUserDto>>(true, items, "Success");
    }

    public async Task<Response<bool>> RemoveWhitelistUserAsync(AddWhitelistRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<bool>(false, false, "UserId and loginUserId are required");
        }

        var employeeId = request.UserId.Trim();
        var whitelist = await _db.Whitelists.FirstOrDefaultAsync(w => w.EmployeeId == employeeId);
        if (whitelist is not null)
        {
            _db.Whitelists.Remove(whitelist);
            await _db.SaveChangesAsync();
        }

        return new Response<bool>(true, true, "User removed from whitelist successfully");
    }

    private async Task<EmployeeListItemDto> ToEmployeeListItemAsync(string employeeId)
    {
        return await _db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Factory)
            .Where(e => e.EmployeeId == employeeId)
            .Select(e => new EmployeeListItemDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.DepartmentName,
                FactoryId = e.FactoryId,
                FactoryName = e.Factory.FactoryName,
                CardNumber = e.CardNumber.HasValue ? e.CardNumber.Value.ToString() : string.Empty,
                RecordStatus = e.RecordStatus,
                IsWhiteList = e.Whitelists.Any(w => w.RecordStatus == 1),
                CreateId = e.CreateId,
                CreateDate = e.CreateDate,
                UpdateId = e.UpdateId,
                UpdateDate = e.UpdateDate
            })
            .FirstAsync();
    }

    private static string? NormalizePhoto(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        var marker = "base64,";
        var markerIndex = trimmed.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return markerIndex >= 0 ? trimmed[(markerIndex + marker.Length)..] : trimmed;
    }
}
