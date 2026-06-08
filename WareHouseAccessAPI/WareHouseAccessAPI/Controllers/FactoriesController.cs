using Microsoft.AspNetCore.Mvc;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Services;

using WarehouseAccessAPI.Attributes;

namespace WarehouseAccessAPI.Controllers;

[ApiController]
[Route("WarehouseAccess/[controller]/[action]")]
public class FactoriesController : ControllerBase
{
    private readonly FactoryService _factoryService;

    public FactoriesController(FactoryService factoryService)
    {
        _factoryService = factoryService;
    }

    [HttpGet]
    public async Task<ActionResult<Response<List<FactoryItemDto>>>> GetFactories()
    {
        var result = await _factoryService.GetFactoriesAsync();
        return Ok(result);
    }

    [HttpPost]
    [MasterDataAuthorize]
    public async Task<ActionResult<Response<FactoryItemDto>>> CreateFactory([FromBody] FactoryUpsertRequestDto request)
    {
        var result = await _factoryService.CreateFactoryAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("required") || result.Message.Contains("invalid"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpPut]
    [MasterDataAuthorize]
    public async Task<ActionResult<Response<FactoryItemDto>>> UpdateFactory([FromBody] FactoryUpsertRequestDto request)
    {
        var result = await _factoryService.UpdateFactoryAsync(request);
        if (!result.Success)
        {
            if (result.Message == "Factory not found")
            {
                return NotFound(result);
            }
            if (result.Message.Contains("required"))
            {
                return BadRequest(result);
            }
            return Conflict(result);
        }
        return Ok(result);
    }

    [HttpDelete]
    [MasterDataAuthorize]
    public async Task<ActionResult<Response<bool>>> DeleteFactory([FromQuery] string? factoryId)
    {
        if (string.IsNullOrWhiteSpace(factoryId))
        {
            return BadRequest(new Response<bool>(false, false, "factoryId is required"));
        }

        var result = await _factoryService.DeleteFactoryAsync(factoryId);
        if (!result.Success)
        {
            if (result.Message == "Factory not found")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
