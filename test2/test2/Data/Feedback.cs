using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Feedback
{
    public string FeedbackId { get; set; } = null!;

    public string? Did { get; set; }

    public string? Pid { get; set; }

    public string? Name { get; set; }

    public DateTime? DateCmt { get; set; }

    public int? Star { get; set; }

    public string? Description { get; set; }

    public virtual Doctor? DidNavigation { get; set; }

    public virtual Patient? PidNavigation { get; set; }
}
