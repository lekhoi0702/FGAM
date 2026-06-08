using System;
using System.Collections.Generic;

namespace FGAMApi.Models;

public partial class Transaction
{
    public string FactoryId { get; set; } = null!;

    public long TransactionId { get; set; }

    public string? CompanyName { get; set; }

    public string EmployeeId { get; set; } = null!;

    public string? EmployeeName { get; set; }

    public long? CardNumber { get; set; }

    public string? Photo { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckoutTime { get; set; }

    public int RecordStatus { get; set; }

    public string? CreateId { get; set; }

    public DateTime CreateDate { get; set; }

    public string? UpdateId { get; set; }

    public DateTime UpdateDate { get; set; }

    public string? Purpose { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Factory Factory { get; set; } = null!;
}
