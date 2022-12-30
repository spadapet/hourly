using Hourly.Data;

namespace Hourly.Utility
{
    public static class PayPeriodUtility
    {
        public static PayPeriod NewPayPeriod(DateTime startDayLocal, PayPeriodType type)
        {
            PayPeriod payPeriod = new()
            {
                FirstDayLocal = startDayLocal,
                DayCount = type.DaysPerPeriod(),
            };

            for (int i = 0; i < payPeriod.DayCount; i++)
            {
                payPeriod.Days.Add(new Day()
                {
                    DayLocal = startDayLocal.AddDays(i),
                });
            }

            return payPeriod;
        }
    }
}
