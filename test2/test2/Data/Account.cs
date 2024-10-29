using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Account
{
    public string Id { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public int? Role { get; set; }

    public bool? Status { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Staff? Staff { get; set; }
}
