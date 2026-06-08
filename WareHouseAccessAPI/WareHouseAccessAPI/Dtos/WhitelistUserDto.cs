namespace WarehouseAccessAPI.Dtos;

public class WhitelistUserDto
{
    public string UserId { get; set; } = null!;
    public string? FullName { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Avatar { get; set; }
}
