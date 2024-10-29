using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Specialty
{
    public string SpecialtyId { get; set; } = null!;

    public string? SpecialtyName { get; set; }

    public string? SpecialtyImg { get; set; }

    public string? ShortDescription { get; set; }

    public virtual ICollection<DetailSpecialty> DetailSpecialties { get; set; } = new List<DetailSpecialty>();

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
