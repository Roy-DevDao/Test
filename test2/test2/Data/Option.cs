using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Option
{
    public string OptionId { get; set; } = null!;

    public string? Did { get; set; }

    public string? Status { get; set; }

    public DateTime? DateWork { get; set; }

    public virtual Doctor? DidNavigation { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
