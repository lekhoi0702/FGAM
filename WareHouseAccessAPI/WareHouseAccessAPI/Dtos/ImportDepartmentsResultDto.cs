namespace WarehouseAccessAPI.Dtos;

public class ImportDepartmentsResultDto
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<ImportDepartmentsErrorDto> Errors { get; set; } = new();
}
