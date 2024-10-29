using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Contact
{
    public string ContactId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }
}
