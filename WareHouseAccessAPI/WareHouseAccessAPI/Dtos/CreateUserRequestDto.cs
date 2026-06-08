namespace WarehouseAccessAPI.Dtos;

public class CreateUserRequestDto
{
    public string? LoginUserId { get; set; }
    public string? UserId { get; set; }
    public string? DepartmentCode { get; set; }
    public string? FactoryId { get; set; }
    public string? FullName { get; set; }
    public bool? IsLoginUser { get; set; }
    public string? CardNumber { get; set; }
    public string? Password { get; set; }
}
