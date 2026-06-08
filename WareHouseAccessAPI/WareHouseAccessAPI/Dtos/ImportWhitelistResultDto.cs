using System.Collections.Generic;

namespace WarehouseAccessAPI.Dtos;

public class ImportWhitelistResultDto
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<ImportWhitelistErrorDto> Errors { get; set; } = new();
}
