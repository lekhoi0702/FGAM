namespace WarehouseAccessAPI.Dtos;

public class CreateEmployeeRequestDto
{
    public string? LoginUserId { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentId { get; set; }
    public string? FactoryId { get; set; }
    public string? CardNumber { get; set; }
}
