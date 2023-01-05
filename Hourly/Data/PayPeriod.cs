namespace Hourly.Data;

public enum PayPeriodType
{
    None,
    Weekly,
    BiWeekly,
}

public enum TimeType
{
    None,
    Work,
    Sick,
    Vacation,
    Holiday,
}

public sealed class PayPeriod
{
    public List<Day> Days { get; } = new();
    public string Notes { get; set; }
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
