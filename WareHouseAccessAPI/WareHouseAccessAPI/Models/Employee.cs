using System;
using System.Collections.Generic;

namespace WarehouseAccessAPI.Models;

public partial class Employee
{
    public string DepartmentId { get; set; } = null!;

    public string FactoryId { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public long? CardNumber { get; set; }

    public int RecordStatus { get; set; }

    public string? CreateId { get; set; }

    public DateTime CreateDate { get; set; }

    public string? UpdateId { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual Department Department { get; set; } = null!;

    public virtual Factory Factory { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
}
