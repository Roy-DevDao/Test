using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class HealthRecord
{
    public string RecordId { get; set; } = null!;

    public string? Pid { get; set; }

    public string? Did { get; set; }

    public string? Oid { get; set; }

    public string? Diagnosis { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }

    public DateTime? DateExam { get; set; }

    public virtual Doctor? DidNavigation { get; set; }

    public virtual Order? OidNavigation { get; set; }

    public virtual Patient? PidNavigation { get; set; }
}
