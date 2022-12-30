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
        return DateTime.UtcNow.PayPeriodIndex(type, firstWorkDayLocal);
    }

    public static int PayPeriodIndex(this DateTime timeUtc, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        DateTime dayLocal = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, Program.TimeZone).Date;
        return (dayLocal - firstWorkDayLocal).Days / type.DaysPerPeriod();
    }

    public static DateTime IndexToPayPeriodStartLocal(int index, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return firstWorkDayLocal.AddDays(index * type.DaysPerPeriod());
    }

    public static string DayToDisplayString(this DateTime dayLocal)
    {
        return dayLocal.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
    }

    public static string DayToWeekdayDisplayString(this DateTime dayLocal)
    {
        return dayLocal.ToString("dddd", CultureInfo.CurrentCulture);
    }

    public static string DayToPersistString(this DateTime dayLocal)
    {
        return dayLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static DateTime PersistStringToDay(string dayString)
    {
        return DateTime.ParseExact(dayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
