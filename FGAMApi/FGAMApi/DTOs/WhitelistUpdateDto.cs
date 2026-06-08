namespace FGAMApi.DTOs
{
    public class WhitelistUpdateDto
    {
        public string FactoryId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }
}
