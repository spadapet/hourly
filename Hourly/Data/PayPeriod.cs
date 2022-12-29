namespace Hourly.Data;

public enum PayPeriodType
{
    None,
    Weekly,
    BiWeekly,
    SemiMonthly,
    Monthly,
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
    public DateTime FirstDay { get; set; }
    public int DayCount { get; set; }
    public List<Day> Days { get; } = new();
}

public sealed class Day
{
    public string Notes { get; set; }
    public List<Time> Times { get; set; } = new();
}

public sealed class Time
{
    public TimeType Type { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
}
