using Microsoft.AspNetCore.Http;

namespace WarehouseAccessAPI.Dtos;

public class ImportWhitelistFormRequestDto
{
    public string? LoginUserId { get; set; }
    public IFormFile? File { get; set; }
}
