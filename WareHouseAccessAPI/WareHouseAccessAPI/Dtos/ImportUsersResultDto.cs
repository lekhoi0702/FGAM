 namespace WarehouseAccessAPI.Dtos;

public class ImportUsersResultDto
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<ImportUsersErrorDto> Errors { get; set; } = new();
}
