using System;
using System.Collections.Generic;

namespace test2.Data;

public partial class Schedule
{
    public string ScheduleId { get; set; } = null!;

    public DateOnly? DateWork { get; set; }

    public TimeOnly? TimeStart { get; set; }

    public TimeOnly? TimeEnd { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
