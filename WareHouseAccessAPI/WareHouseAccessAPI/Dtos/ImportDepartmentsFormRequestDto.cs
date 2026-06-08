namespace WarehouseAccessAPI.Dtos;

public class ImportDepartmentsFormRequestDto
{
    public string? LoginUserId { get; set; }
    public string? CompanyId { get; set; }
    public IFormFile? File { get; set; }
}
