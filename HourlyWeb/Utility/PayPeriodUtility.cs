using Hourly.Data;

namespace Hourly.Utility;

public static class PayPeriodUtility
{
    public static string RowKey(this PayPeriod payPeriod, User user, bool admin)
    {
        return user.TimeToPayPeriodStartLocal(payPeriod.Days[0].DayLocal).DayToRowKey(admin);
    }

    public static PayPeriod NewPayPeriod(DateTime startDayLocal, PayPeriodType type)
    {
        PayPeriod payPeriod = new();

        for (int i = 0; i < type.DaysPerPeriod(); i++)
        {
            payPeriod.Days.Add(new Day()
            {
                DayLocal = startDayLocal.Date.AddDays(i),
            });
        }

        return payPeriod;
    }

    public static void MergeDays(this PayPeriod mergeInto, PayPeriod mergeFrom)
    {
        foreach (Day dayFrom in mergeFrom.Days)
        {
            if (mergeInto.Days.FirstOrDefault(d => d.DayLocal == dayFrom.DayLocal) is Day dayInto)
            {
                foreach (Time timeFrom in dayFrom.Times)
                {
                    if (dayInto.Times.FirstOrDefault(t => t.StartLocal == timeFrom.StartLocal) is Time timeInto)
                    {
                        if (!timeInto.EndLocal.HasValue)
                        {
                            timeInto.EndLocal = timeFrom.EndLocal;
                        }

                        if (timeInto.Type == TimeType.None)
                        {
                            timeInto.Type = timeFrom.Type;
                        }
                    }
                    else
                    {
                        dayInto.Times.Add(timeFrom);
                    }
                }
            }
            else
            {
                mergeInto.Days.Add(dayFrom);
            }
        }

        foreach (Day day in mergeInto.Days)
        {
            foreach (Time time in day.Times)
            {
                time.StartLocal = time.StartLocal.MoveTimeToDay(day.DayLocal);

                if (time.EndLocal.HasValue)
                {
                    time.EndLocal = time.EndLocal.Value.MoveTimeToDay(day.DayLocal);
                }
            }

            day.Times.RemoveAll(t => t.EndLocal.HasValue && t.EndLocal.Value < t.StartLocal);
            day.Times.Sort((l, r) => l.StartLocal.CompareTo(r.StartLocal));
        }

        mergeInto.Days.Sort((l, r) => l.DayLocal.CompareTo(r.DayLocal));
    }

    public static PayPeriod Canonicalize(this PayPeriod payPeriod)
    {
        PayPeriod fixedPayPeriod = new()
        {
            Notes = payPeriod.Notes,
            PrivateNotes = payPeriod.PrivateNotes,
            PayRate = payPeriod.PayRate,
        };

        fixedPayPeriod.MergeDays(payPeriod);

        return fixedPayPeriod;
    }

    public static void PunchClock(this PayPeriod payPeriod, DateTime punchTime)
    {
        if (payPeriod.Days.FirstOrDefault(d => d.DayLocal.Date == punchTime.Date) is Day day)
        {
            if (day.Times.FirstOrDefault(t => t.Type == TimeType.Work && !t.EndLocal.HasValue) is Time existingTime)
            {
                existingTime.EndLocal = punchTime;
            }
            else
            {
                day.Times.Add(new Time()
                {
                    Type = TimeType.Work,
                    StartLocal = punchTime,
                });
            }
        }
        else
        {
            throw new InvalidOperationException($"Pay period is missing day: {punchTime}");
        }
    }

    public static (double, double) ComputeNormalAndOvertimeHours(this PayPeriod payPeriod)
    {
        int currentWeek = 0;
        double currentWeekRegular = 0;
        double regular = 0;
        double overtime = 0;

        foreach (Day day in payPeriod.Days)
        {
            int week = (int)(day.DayLocal - payPeriod.Days[0].DayLocal).TotalDays / 7;
            if (week != currentWeek)
            {
                currentWeek = week;
                currentWeekRegular = 0;
            }

            foreach (Time time in day.Times.Where(t => t.Type == TimeType.Work && t.EndLocal.HasValue && t.EndLocal.Value > t.StartLocal))
            {
                double hours = (time.EndLocal.Value - time.StartLocal).TotalHours;

                if (currentWeekRegular + hours > 40)
                {
                    regular += 40 - currentWeekRegular;
                    overtime += (currentWeekRegular + hours) - 40;
                    currentWeekRegular = 40;
                }
                else
                {
                    regular += hours;
                    currentWeekRegular += hours;
                }
            }
        }

        return (regular, overtime);
    }

    /// <summary>
    /// Adds hours of a certain type among all days in the pay period
    /// </summary>
    public static double HoursFor(this PayPeriod payPeriod, params TimeType[] types)
    {
        double hours = 0;

        foreach (Day day in payPeriod.Days)
        {
            foreach (Time time in day.Times.Where(t => Array.IndexOf(types, t.Type) != -1 && t.EndLocal.HasValue && t.EndLocal.Value > t.StartLocal))
            {
                hours += (time.EndLocal.Value - time.StartLocal).TotalHours;
            }
        }

        return hours;
    }

    /// <summary>
    /// Returns hours for display
    /// </summary>
    public static double HoursFor(this PayPeriod payPeriod, TimeDisplayType type)
    {
        return type switch
        {
            TimeDisplayType.Regular => payPeriod.ComputeNormalAndOvertimeHours().Item1,
            TimeDisplayType.Overtime => payPeriod.ComputeNormalAndOvertimeHours().Item2,
            TimeDisplayType.Vacation => payPeriod.HoursFor(TimeType.Vacation),
            TimeDisplayType.Holiday => payPeriod.HoursFor(TimeType.Holiday),
            TimeDisplayType.Sick => payPeriod.HoursFor(TimeType.Sick),
            TimeDisplayType.Total => payPeriod.HoursFor(TimeType.Work, TimeType.Vacation, TimeType.Holiday, TimeType.Sick),
            _ => throw new InvalidOperationException($"Unexpected TimeDisplayType: {type}")
        };
    }

    /// <summary>
    /// Returns dollars to display
    /// </summary>
    public static double PayFor(this PayPeriod payPeriod, TimeDisplayType type)
    {
        if (payPeriod.PayRate.HasValue)
        {
            return type switch
            {
                TimeDisplayType.Overtime => payPeriod.HoursFor(type) * payPeriod.PayRate.Value * 1.5,
                TimeDisplayType.Total =>
                    payPeriod.HoursFor(type) * payPeriod.PayRate.Value +
                    payPeriod.HoursFor(TimeDisplayType.Overtime) * payPeriod.PayRate.Value * 0.5,
                _ => payPeriod.HoursFor(type) * payPeriod.PayRate.Value
            };
        }

        return 0;
    }
}
