using System;

namespace WarehouseAccessAPI.Dtos;

public class TransactionDetailDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string? CardNumber { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? FactoryId { get; set; }
    public string? FactoryName { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? ContactPerson { get; set; }
    public string? FactoryNameAlias { get; set; }
    public string? Purpose { get; set; }
    public string? PurposeName => Purpose;
    public string? Photo { get; set; }
    public DateTimeOffset? CheckInTime { get; set; }
    public DateTimeOffset? CheckoutTime { get; set; }
    public DateTimeOffset? CreateDate { get; set; }
    public DateTimeOffset? UpdateDate { get; set; }
}
