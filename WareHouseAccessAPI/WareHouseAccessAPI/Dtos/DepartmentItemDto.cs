namespace WarehouseAccessAPI.Dtos;

public class DepartmentItemDto
{
    public string DepartmentId { get; set; } = string.Empty;
    public string FactoryId { get; set; } = string.Empty;
    public string? FactoryName { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int RecordStatus { get; set; }
}
