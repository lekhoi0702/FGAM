namespace WarehouseAccessAPI.Dtos;

public class CreateTransactionCheckInRequestDto
{
    public string? LoginUserId { get; set; }
    public string? CardNumber { get; set; }
    public string? EmployeeId { get; set; }
    public string? DepartmentId { get; set; }
    public string? FactoryId { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? PurposeName { get; set; }
    public string? Photo { get; set; }
    public string? ContactPerson { get; set; }
}
