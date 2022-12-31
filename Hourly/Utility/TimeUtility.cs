using Hourly.Data;
using System.Globalization;

namespace Hourly.Utility;

public static class TimeUtility
{
    public static int DaysPerPeriod(this PayPeriodType type)
    {
        switch (type)
        {
            case PayPeriodType.Weekly: return 7;
            case PayPeriodType.BiWeekly: return 14;
            default: throw new InvalidOperationException($"Pay period has no set number of days: {type}");
        }
    }

    public static int CurrentPayPeriodIndex(this PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return DateTime.Now.PayPeriodIndex(type, firstWorkDayLocal);
    }

    public static int PayPeriodIndex(this DateTime timeLocal, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return (timeLocal.Date > firstWorkDayLocal)
            ? (timeLocal.Date - firstWorkDayLocal).Days / type.DaysPerPeriod()
            : 0;
    }

    public static DateTime IndexToPayPeriodStartLocal(int index, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return firstWorkDayLocal.AddDays(index * type.DaysPerPeriod());
    }

    public static DateTime TimeToPayPeriodStartLocal(DateTime timeLocal, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return TimeUtility.IndexToPayPeriodStartLocal(timeLocal.PayPeriodIndex(type, firstWorkDayLocal), type, firstWorkDayLocal);
    }

    public static string DayToNoYearDisplayString(this DateTime dayLocal)
    {
        return dayLocal.ToString("MMM d", CultureInfo.InvariantCulture);
    }

    public static string DayToDisplayString(this DateTime dayLocal)
    {
        return dayLocal.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
    }

    public static string DayToWeekdayDisplayString(this DateTime dayLocal)
    {
        return dayLocal.ToString("MMM d, ddd", CultureInfo.CurrentCulture);
    }

    public static string DayToPersistString(this DateTime dayLocal)
    {
        return dayLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string TimeToDisplayString(this DateTime timeLocal)
    {
        return timeLocal.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }
}
