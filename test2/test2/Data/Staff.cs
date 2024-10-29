using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Staff
{
    public string StaffId { get; set; } = null!;

    public string? Name { get; set; }

    public string? StaffImg { get; set; }

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public virtual Account StaffNavigation { get; set; } = null!;
}
