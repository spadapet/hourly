using Azure.Data.Tables;
using Hourly.Data;

namespace Hourly.Utility;

public static class UserUtility
{
    public static async Task<Users> GetUsersAsync(TableClient tableClient)
    {
        DataEntity usersEntity = await tableClient.GetEntityAsync<DataEntity>("0", typeof(Users).Name);
        Users users = usersEntity.Deserialize<Users>();

        if (Program.IsDevelopment)
        {
            users.ById["testUser"] = new()
            {
                Name = "Test User",
                Password = "testPassword",
                AdminPassword = "testAdminPassword",
                Partition = "Test",
                PayPeriodType = PayPeriodType.Weekly,
                FirstWorkDayLocal = TimeUtility.StringToDay("2022-10-29"),
                PayRate = 25,
            };
        }

        return users;
    }

    public static int DaysPerPeriod(this User user)
    {
        return user.PayPeriodType.DaysPerPeriod();
    }

    public static int CurrentPayPeriodIndex(this User user)
    {
        return user.PayPeriodType.CurrentPayPeriodIndex(user.FirstWorkDayLocal);
    }

    public static int PayPeriodIndex(this User user, DateTime timeLocal)
    {
        return timeLocal.PayPeriodIndex(user.PayPeriodType, user.FirstWorkDayLocal);
    }

    public static DateTime IndexToPayPeriodStartLocal(this User user, int index)
    {
        return TimeUtility.IndexToPayPeriodStartLocal(index, user.PayPeriodType, user.FirstWorkDayLocal);
    }

    public static DateTime TimeToPayPeriodStartLocal(this User user, DateTime timeLocal)
    {
        return TimeUtility.TimeToPayPeriodStartLocal(timeLocal, user.PayPeriodType, user.FirstWorkDayLocal);
    }
}
