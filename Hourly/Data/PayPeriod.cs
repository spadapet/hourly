namespace Hourly.Data;

public enum PayPeriodType
{
    // NO CHANGES ALLOWED EXCEPT APPENDING.
    // The integer values could be persisted.
    None,
    Weekly,
    BiWeekly,
}

public enum TimeType
{
    // NO CHANGES ALLOWED EXCEPT APPENDING.
    // The integer values could be persisted.
    None,
    Work,
    Sick,
    Vacation,
    Holiday,
    Deleted,
}

/// <summary>
/// Only for UI display
/// </summary>
public enum TimeDisplayType
{
    None,
    Regular,
    Overtime,
    Vacation,
    Holiday,
    Sick,
    Total,
}

public sealed class PayPeriod
{
    public List<Day> Days { get; } = new();
    public string Notes { get; set; }
    public string PrivateNotes { get; set; }
    public double? PayRate { get; set; }
}

public sealed class Day
{
    public DateTime DayLocal { get; set; }
    public List<Time> Times { get; set; } = new();
}

public sealed class Time
{
    public TimeType Type { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime? EndLocal { get; set; }
}
