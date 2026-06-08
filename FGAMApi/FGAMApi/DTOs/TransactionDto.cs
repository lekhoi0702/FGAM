namespace FGAMApi.DTOs
{
    public class TransactionDto
    {
        public string FactoryId { get; set; } = string.Empty;
        public string FactoryName { get; set; } = string.Empty;
        public long TransactionId { get; set; }
        public string? CompanyName { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public long? CardNumber { get; set; }
        public string? Photo { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public string? Purpose { get; set; }
    }
}
