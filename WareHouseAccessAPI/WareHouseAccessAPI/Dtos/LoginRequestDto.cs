namespace WarehouseAccessAPI.Dtos;

public class LoginRequestDto
{
    public string? UserId { get; set; }
    public string? FactoryId { get; set; }
    public string? Password { get; set; }
}
