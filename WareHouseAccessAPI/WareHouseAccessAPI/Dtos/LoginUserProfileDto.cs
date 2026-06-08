namespace WarehouseAccessAPI.Dtos;

public class LoginUserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? DepartmentName { get; set; }
    public string? FactoryId { get; set; }
    public string? FactoryName { get; set; }
    public string? RecordStatus { get; set; }
}
