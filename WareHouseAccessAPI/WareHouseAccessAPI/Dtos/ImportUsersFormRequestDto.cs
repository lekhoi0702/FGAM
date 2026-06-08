namespace WarehouseAccessAPI.Dtos;

public class ImportUsersFormRequestDto
{
    public string? LoginUserId { get; set; }
    public IFormFile? File { get; set; }
}
