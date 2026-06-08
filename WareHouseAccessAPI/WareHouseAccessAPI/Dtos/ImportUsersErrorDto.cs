namespace WarehouseAccessAPI.Dtos;

public class ImportUsersErrorDto
{
    public int RowNumber { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
