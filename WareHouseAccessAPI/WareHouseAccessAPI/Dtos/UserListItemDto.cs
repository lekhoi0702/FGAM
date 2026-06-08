namespace WarehouseAccessAPI.Dtos;

public class UserListItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string? CardNumber { get; set; }
    public string? FullName { get; set; }
    public string? FactoryId { get; set; }
    public string? FactoryName { get; set; }
    public string? DepartmentCode { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsLoginUser { get; set; }
    public bool IsWhiteList { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
