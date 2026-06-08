namespace WarehouseAccessAPI.Dtos;

public class ImportDepartmentsErrorDto
{
    public int RowNumber { get; set; }
    public string DepartmentId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
