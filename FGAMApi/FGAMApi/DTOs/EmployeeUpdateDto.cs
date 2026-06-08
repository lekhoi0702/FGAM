namespace FGAMApi.DTOs
{
    public class EmployeeUpdateDto
    {
        public string FactoryId { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public long? CardNumber { get; set; }
    }
}
