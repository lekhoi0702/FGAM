namespace FGAMApi.DTOs
{
    public class EmployeeCreateDto
    {
        public string FactoryId { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public long? CardNumber { get; set; }
    }
}
