using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Order
{
    public string Oid { get; set; } = null!;

    public string? Pid { get; set; }

    public string? OptionId { get; set; }

    public string? Status { get; set; }

    public DateTime? DateOrder { get; set; }

    public string? Symptom { get; set; }

    public virtual ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();

    public virtual Option? Option { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Patient? PidNavigation { get; set; }
}
