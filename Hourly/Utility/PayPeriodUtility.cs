using Hourly.Data;

namespace Hourly.Utility;

public static class PayPeriodUtility
{
    public static string RowKey(this PayPeriod payPeriod, User user, bool admin)
    {
        return user.TimeToPayPeriodStartLocal(payPeriod.Days[0].DayLocal).DayToRowKey(admin);
    }

    public static PayPeriod NewPayPeriod(DateTime startDayLocal, PayPeriodType type, double payRate)
    {
        PayPeriod payPeriod = new()
        {
            PayRate = payRate,
        };

        for (int i = 0; i < type.DaysPerPeriod(); i++)
        {
            payPeriod.Days.Add(new Day()
            {
                DayLocal = startDayLocal.Date.AddDays(i),
            });
        }

        return payPeriod;
    }

    public static void Merge(this PayPeriod mergeInto, PayPeriod mergeFrom)
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

        fixedPayPeriod.Merge(payPeriod);

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
}
