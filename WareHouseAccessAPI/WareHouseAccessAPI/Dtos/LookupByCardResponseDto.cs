namespace WarehouseAccessAPI.Dtos;

public class LookupByCardResponseDto
{
    public string? CardNumber { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? FactoryId { get; set; }
    public string? FactoryName { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? PurposeName { get; set; }
    public bool IsInside { get; set; }
    public string? OpenTransactionId { get; set; }
    public bool IsExternalGuestDataApplied { get; set; }
    public string? ContactPerson { get; set; }
}
