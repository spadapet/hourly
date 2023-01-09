using Azure;
using Azure.Data.Tables;
using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class TimeChart : ComponentBase
{
    [Parameter]
    public ViewModel ViewModel { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    private long disabledCount;
    private bool Disabled => Interlocked.Read(ref this.disabledCount) > 0;
    private PayPeriod MergedPayPeriod { get; set; }
    private PayPeriod UserPayPeriod { get; set; }

    private enum TimeDisplayType
    {
        Regular,
        Overtime,
        Vacation,
        Holiday,
        Sick,
        Total,
    }

    protected override async Task OnParametersSetAsync()
    {
        this.ViewModel.PunchClockAction = this.PunchClock;

        await this.DisableDuring(async () =>
        {
            await this.SaveChanges();

            DateTime startDayLocal = this.ViewModel.User.TimeToPayPeriodStartLocal(this.ViewModel.ForDayLocal);
            NullableResponse<DataEntity> existingAdminEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.ViewModel.User.Partition, startDayLocal.DayToRowKey(admin: true));
            NullableResponse<DataEntity> existingUserEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.ViewModel.User.Partition, startDayLocal.DayToRowKey(admin: false));

            this.MergedPayPeriod = existingAdminEntity.HasValue
                ? existingAdminEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType, this.ViewModel.User.PayRate);

            this.UserPayPeriod = existingUserEntity.HasValue
                ? existingUserEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType, this.ViewModel.User.PayRate);

            this.MergedPayPeriod.Merge(this.UserPayPeriod);
        });

        await base.OnParametersSetAsync();
    }

    private async Task SaveChanges()
    {
        if ((this.ViewModel.Admin ? this.MergedPayPeriod : this.UserPayPeriod) is PayPeriod payPeriodToUpdate)
        {
            payPeriodToUpdate = payPeriodToUpdate.Canonicalize();

            await this.DisableDuring(async () =>
            {
                DataEntity entity = new(payPeriodToUpdate, this.ViewModel.User, this.ViewModel.Admin);
                Response response = await this.TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
                if (response.IsError)
                {
                    throw new InvalidOperationException($"Failed to save entity: {entity.PartitionKey}, {entity.RowKey}");
                }
            });
        }
    }

    private async Task ResetChanges()
    {
        this.MergedPayPeriod = null;
        this.UserPayPeriod = null;
        await this.DisableDuring(this.OnParametersSetAsync);
    }

    private static void NewTime(Day day)
    {
        day.Times.Add(new Time()
        {
            Type = TimeType.Work,
            StartLocal = TimeUtility.LocalNow.MoveTimeToDay(day.DayLocal),
        });
    }

    private static void DeleteTime(Day day, Time time)
    {
        day.Times.Remove(time);
    }

    private async Task PunchClock()
    {
        DateTime punchTime = TimeUtility.LocalNow;

        await this.DisableDuring(async () =>
        {
            DateTime startDayLocal = this.ViewModel.User.TimeToPayPeriodStartLocal(punchTime);
            NullableResponse<DataEntity> existingEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.ViewModel.User.Partition, startDayLocal.DayToRowKey(admin: false));

            PayPeriod payPeriod = existingEntity.HasValue
                ? existingEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType, this.ViewModel.User.PayRate);

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

                DataEntity entity = new(payPeriod, this.ViewModel.User, admin: false);
                Response response = await this.TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);

                if (response.IsError)
                {
                    throw new InvalidOperationException($"Failed to save entity: {entity.PartitionKey}, {entity.RowKey}");
                }

                this.MergedPayPeriod.Merge(payPeriod);
                this.ViewModel.ForDayLocal = punchTime.Date;
            }
        });
    }

    private double HoursFor(TimeDisplayType type)
    {
        return type switch
        {
            TimeDisplayType.Regular => 1,
            TimeDisplayType.Overtime => 2,
            TimeDisplayType.Vacation => 3,
            TimeDisplayType.Holiday => 4,
            TimeDisplayType.Sick => 5,
            TimeDisplayType.Total =>
                this.HoursFor(TimeDisplayType.Regular) +
                this.HoursFor(TimeDisplayType.Overtime) +
                this.HoursFor(TimeDisplayType.Vacation) +
                this.HoursFor(TimeDisplayType.Holiday) +
                this.HoursFor(TimeDisplayType.Sick),
            _ => throw new InvalidOperationException($"Unexpected TimeDisplayType: {type}")
        };
    }

    private double PayFor(TimeDisplayType type)
    {
        return type switch
        {
            TimeDisplayType.Overtime => this.HoursFor(type) * this.MergedPayPeriod.PayRate * 1.5,
            TimeDisplayType.Total =>
                this.HoursFor(type) * this.MergedPayPeriod.PayRate +
                this.HoursFor(TimeDisplayType.Overtime) * this.MergedPayPeriod.PayRate * 0.5,
            _ => this.HoursFor(type) * this.MergedPayPeriod.PayRate
        };
    }

    private async Task DisableDuring(Func<Task> taskFunc)
    {
        Interlocked.Increment(ref this.disabledCount);
        try
        {
            await taskFunc();
        }
        finally
        {
            Interlocked.Decrement(ref this.disabledCount);
        }
    }
}
