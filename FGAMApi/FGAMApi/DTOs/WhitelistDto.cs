namespace FGAMApi.DTOs
{
    public class WhitelistDto
    {
        public int WhitelistId { get; set; }
        public string FactoryId { get; set; } = string.Empty;
        public string FactoryName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }
}
