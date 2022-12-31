﻿namespace Hourly.Data;

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
    public DateTime FirstDayLocal { get; set; }
    public int DayCount { get; set; }
    public List<Day> Days { get; } = new();
}

public sealed class Day
{
    public string Notes { get; set; }
    public DateTime DayLocal { get; set; }
    public List<Time> Times { get; set; } = new();
}

public sealed class Time
{
    public string Notes { get; set; }
    public TimeType Type { get; set; }
    public DateTime? StartLocal { get; set; }
    public DateTime? EndLocal { get; set; }
}
