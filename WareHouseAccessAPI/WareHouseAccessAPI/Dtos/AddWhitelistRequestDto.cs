namespace WarehouseAccessAPI.Dtos;

public class AddWhitelistRequestDto
{
    public string UserId { get; set; } = null!;
    public string? Avatar { get; set; }
    public string LoginUserId { get; set; } = null!;
}
