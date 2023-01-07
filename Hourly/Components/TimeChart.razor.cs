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
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType);

            this.UserPayPeriod = existingUserEntity.HasValue
                ? existingUserEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType);

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
                Response response = await this.TableClient.UpsertEntityAsync(entity);
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

    public async Task PunchClock()
    {
        await Task.Yield();
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
