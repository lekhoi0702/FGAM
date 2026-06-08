using System;
using System.Collections.Generic;

namespace FGAMApi.Models;

public partial class Department
{
    public string DepartmentId { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public int RecordStatus { get; set; }

    public string? CreateId { get; set; }

    public DateTime CreateDate { get; set; }

    public string? UpdateId { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
