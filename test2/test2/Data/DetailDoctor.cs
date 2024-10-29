using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class DetailDoctor
{
    public string DetailId { get; set; } = null!;

    public string? Did { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public virtual Doctor? DidNavigation { get; set; }
}
