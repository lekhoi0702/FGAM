using System;
using System.Collections.Generic;

namespace FGAMApi.Models;

public partial class Factory
{
    public string FactoryId { get; set; } = null!;

    public string FactoryName { get; set; } = null!;

    public int RecordStatus { get; set; }

    public string? CreateId { get; set; }

    public DateTime CreateDate { get; set; }

    public string? UpdateId { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
}
