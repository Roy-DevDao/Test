using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Payment
{
    public string PayId { get; set; } = null!;

    public string? Oid { get; set; }

    public string? Method { get; set; }

    public string? PayImg { get; set; }

    public DateTime? DatePay { get; set; }

    public virtual Order? OidNavigation { get; set; }
}
