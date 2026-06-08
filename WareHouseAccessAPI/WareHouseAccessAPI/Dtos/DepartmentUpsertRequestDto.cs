namespace WarehouseAccessAPI.Dtos;

public class DepartmentUpsertRequestDto
{
    public string? LoginUserId { get; set; }
    public string? FactoryId { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}
