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

    public static int CurrentPayPeriodIndex(PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return TimeUtility.PayPeriodIndex(DateTime.UtcNow, type, firstWorkDayLocal);
    }

    public static int PayPeriodIndex(DateTime timeUtc, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        DateTime dayLocal = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, Program.TimeZone).Date;
        return (dayLocal - firstWorkDayLocal).Days / type.DaysPerPeriod();
    }

    public static DateTime IndexToPayPeriodStart(int index, PayPeriodType type, DateTime firstWorkDayLocal)
    {
        return firstWorkDayLocal.AddDays(index * type.DaysPerPeriod());
    }

    public static string DayToString(DateTime dayLocal)
    {
        return dayLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static DateTime StringToDay(string dayString)
    {
        return DateTime.ParseExact(dayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
