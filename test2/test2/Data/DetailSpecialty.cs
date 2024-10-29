using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class DetailSpecialty
{
    public string DetailId { get; set; } = null!;

    public string? SpecialtyId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public virtual Specialty? Specialty { get; set; }
}
