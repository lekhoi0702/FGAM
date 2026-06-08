using System;
using System.Collections.Generic;

namespace WarehouseAccessAPI.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string FactoryId { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;

    public string Passwrd { get; set; } = null!;

    public int RecordStatus { get; set; }

    public string? CreateId { get; set; }

    public DateTime CreateDate { get; set; }

    public string? UpdateId { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Factory Factory { get; set; } = null!;
}
