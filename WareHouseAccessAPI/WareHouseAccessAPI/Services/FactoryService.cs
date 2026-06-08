using Microsoft.EntityFrameworkCore;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Services;

public class FactoryService
{
    private readonly FgamContext _db;
    private const int ActiveRecordStatus = 1;

    public FactoryService(FgamContext db)
    {
        _db = db;
    }

    public async Task<Response<List<FactoryItemDto>>> GetFactoriesAsync()
    {
        var items = await _db.Factories
            .AsNoTracking()
            .OrderBy(x => x.FactoryName)
            .Select(x => new FactoryItemDto
            {
                FactoryId = x.FactoryId,
                FactoryName = x.FactoryName,
                RecordStatus = x.RecordStatus
            })
            .ToListAsync();

        return new Response<List<FactoryItemDto>>(true, items, "Success");
    }

    public async Task<Response<FactoryItemDto>> CreateFactoryAsync(FactoryUpsertRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<FactoryItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.FactoryId) || string.IsNullOrWhiteSpace(request.FactoryName))
        {
            return new Response<FactoryItemDto>(false, null, "FactoryId and FactoryName are required");
        }

        var factoryId = request.FactoryId.Trim();
        var factoryName = request.FactoryName.Trim();
        var loginUserId = request.LoginUserId.Trim();

        if (await _db.Factories.AsNoTracking().AnyAsync(x => x.FactoryId == factoryId))
        {
            return new Response<FactoryItemDto>(false, null, "FactoryId already exists");
        }

        if (await _db.Factories.AsNoTracking().AnyAsync(x => x.FactoryName == factoryName))
        {
            return new Response<FactoryItemDto>(false, null, "FactoryName already exists");
        }

        var now = DateTime.Now;
        var entity = new Factory
        {
            FactoryId = factoryId,
            FactoryName = factoryName,
            RecordStatus = ActiveRecordStatus,
            CreateDate = now,
            CreateId = loginUserId,
            UpdateDate = now,
            UpdateId = loginUserId
        };

        _db.Factories.Add(entity);
        await _db.SaveChangesAsync();

        return new Response<FactoryItemDto>(true, new FactoryItemDto
        {
            FactoryId = entity.FactoryId,
            FactoryName = entity.FactoryName,
            RecordStatus = entity.RecordStatus
        }, "Success");
    }

    public async Task<Response<FactoryItemDto>> UpdateFactoryAsync(FactoryUpsertRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LoginUserId))
        {
            return new Response<FactoryItemDto>(false, null, "loginUserId is required");
        }

        if (string.IsNullOrWhiteSpace(request.FactoryId) || string.IsNullOrWhiteSpace(request.FactoryName))
        {
            return new Response<FactoryItemDto>(false, null, "FactoryId and FactoryName are required");
        }

        var factoryId = request.FactoryId.Trim();
        var factoryName = request.FactoryName.Trim();
        var loginUserId = request.LoginUserId.Trim();

        var entity = await _db.Factories.FirstOrDefaultAsync(x => x.FactoryId == factoryId);
        if (entity is null)
        {
            return new Response<FactoryItemDto>(false, null, "Factory not found");
        }

        if (await _db.Factories.AsNoTracking().AnyAsync(x => x.FactoryId != factoryId && x.FactoryName == factoryName))
        {
            return new Response<FactoryItemDto>(false, null, "FactoryName already exists");
        }

        entity.FactoryName = factoryName;
        entity.UpdateDate = DateTime.Now;
        entity.UpdateId = loginUserId;
        
        await _db.SaveChangesAsync();

        return new Response<FactoryItemDto>(true, new FactoryItemDto
        {
            FactoryId = entity.FactoryId,
            FactoryName = entity.FactoryName,
            RecordStatus = entity.RecordStatus
        }, "Success");
    }

    public async Task<Response<bool>> DeleteFactoryAsync(string factoryId)
    {
        if (string.IsNullOrWhiteSpace(factoryId))
        {
            return new Response<bool>(false, false, "FactoryId is required");
        }

        var trimmedFactoryId = factoryId.Trim();
        var entity = await _db.Factories.FirstOrDefaultAsync(x => x.FactoryId == trimmedFactoryId);
        if (entity is null)
        {
            return new Response<bool>(false, false, "Factory not found");
        }


        _db.Factories.Remove(entity);
        await _db.SaveChangesAsync();
        return new Response<bool>(true, true, "Success");
    }
}
