namespace FGAMApi.DTOs
{
    public class TransactionCreateDto
    {
        public string FactoryId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Photo { get; set; }
        public string? Purpose { get; set; }
    }
}
