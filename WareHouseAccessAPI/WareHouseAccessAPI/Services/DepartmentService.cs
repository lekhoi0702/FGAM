using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Services;

public class DepartmentService
{
    private readonly FgamContext _db;
    private const int ActiveRecordStatus = 1;

    public DepartmentService(FgamContext db)
    {
        _db = db;
    }

    public async Task<Response<List<DepartmentItemDto>>> GetDepartmentsAsync()
    {
        var items = await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentItemDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                RecordStatus = d.RecordStatus,
                FactoryId = string.Empty,
                FactoryName = string.Empty
            })
            .ToListAsync();

        return new Response<List<DepartmentItemDto>>(true, items, "Success");
    }

    public async Task<Response<DepartmentItemDto>> CreateDepartmentAsync(DepartmentUpsertRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<DepartmentItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.DepartmentId) || string.IsNullOrWhiteSpace(request.DepartmentName))
        {
            return new Response<DepartmentItemDto>(false, null, "DepartmentId and DepartmentName are required");
        }

        var departmentId = request.DepartmentId.Trim();

        if (await _db.Departments.AsNoTracking().AnyAsync(x => x.DepartmentId == departmentId))
        {
            return new Response<DepartmentItemDto>(false, null, "DepartmentId already exists");
        }

        var loginUserId = request.LoginUserId.Trim();
        var now = DateTime.Now;

        var entity = new Department
        {
            DepartmentId = departmentId,
            DepartmentName = request.DepartmentName.Trim(),
            RecordStatus = ActiveRecordStatus,
            CreateDate = now,
            CreateId = loginUserId,
            UpdateDate = now,
            UpdateId = loginUserId
        };

        _db.Departments.Add(entity);
        await _db.SaveChangesAsync();

        var dto = await ToDepartmentItemDtoAsync(departmentId);
        return new Response<DepartmentItemDto>(true, dto, "Success");
    }

    public async Task<Response<DepartmentItemDto>> UpdateDepartmentAsync(DepartmentUpsertRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<DepartmentItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.DepartmentId) || string.IsNullOrWhiteSpace(request.DepartmentName))
        {
            return new Response<DepartmentItemDto>(false, null, "DepartmentId and DepartmentName are required");
        }

        var departmentId = request.DepartmentId.Trim();
        var entity = await _db.Departments.FirstOrDefaultAsync(x => x.DepartmentId == departmentId);
        if (entity is null)
        {
            return new Response<DepartmentItemDto>(false, null, "Department not found");
        }

        entity.DepartmentName = request.DepartmentName.Trim();
        entity.UpdateDate = DateTime.Now;
        entity.UpdateId = request.LoginUserId.Trim();
        
        await _db.SaveChangesAsync();

        var dto = await ToDepartmentItemDtoAsync(departmentId);
        return new Response<DepartmentItemDto>(true, dto, "Success");
    }

    public async Task<Response<bool>> DeleteDepartmentAsync(string departmentId)
    {
        if (string.IsNullOrWhiteSpace(departmentId))
        {
            return new Response<bool>(false, false, "DepartmentId is required");
        }

        var entity = await _db.Departments.FirstOrDefaultAsync(x => x.DepartmentId == departmentId.Trim());
        if (entity is null)
        {
            return new Response<bool>(false, false, "Department not found");
        }

        _db.Departments.Remove(entity);
        await _db.SaveChangesAsync();
        return new Response<bool>(true, true, "Success");
    }

    private async Task<DepartmentItemDto> ToDepartmentItemDtoAsync(string departmentId)
    {
        return await _db.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentId == departmentId)
            .Select(d => new DepartmentItemDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                RecordStatus = d.RecordStatus,
                FactoryId = string.Empty,
                FactoryName = string.Empty
            })
            .FirstAsync();
    }
}
