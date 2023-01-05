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
            day.Times.Sort((l, r) => l.StartLocal.CompareTo(r.StartLocal));
        }

        mergeInto.Days.Sort((l, r) => l.DayLocal.CompareTo(r.DayLocal));
    }
}
