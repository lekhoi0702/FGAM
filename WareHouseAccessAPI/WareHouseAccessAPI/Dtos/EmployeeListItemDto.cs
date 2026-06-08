using System;

namespace WarehouseAccessAPI.Dtos;

public class EmployeeListItemDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentId { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string FactoryId { get; set; } = string.Empty;
    public string? FactoryName { get; set; }
    public string? CardNumber { get; set; }
    public int RecordStatus { get; set; }
    public bool IsWhiteList { get; set; }
    public string? CreateId { get; set; }
    public DateTime CreateDate { get; set; }
    public string? UpdateId { get; set; }
    public DateTime UpdateDate { get; set; }
}
