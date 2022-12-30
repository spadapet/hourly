using Azure.Data.Tables;
using Hourly.Data;

namespace Hourly.Utility
{
    public static class UserUtility
    {
        public static async Task<Users> GetUsersAsync(TableClient tableClient)
        {
            DataEntity usersEntity = await tableClient.GetEntityAsync<DataEntity>("0", typeof(Users).Name);
            Users users = usersEntity.Deserialize<Users>();

            // Remove test-only users in production
            if (!Program.IsDevelopment)
            {
                foreach (string id in users.ById.Where(p => p.Value.DevOnly).Select(p => p.Key).ToArray())
                {
                    users.ById.Remove(id);
                }
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

        public static int PayPeriodIndex(this User user, DateTime timeUtc)
        {
            return timeUtc.PayPeriodIndex(user.PayPeriodType, user.FirstWorkDayLocal);
        }

        public static DateTime IndexToPayPeriodStartLocal(this User user, int index)
        {
            return TimeUtility.IndexToPayPeriodStartLocal(index, user.PayPeriodType, user.FirstWorkDayLocal);
        }
    }
}
