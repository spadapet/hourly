using Azure;
using Azure.Data.Tables;
using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class TimeChart : ComponentBase
{
    [Parameter]
    public User User { get; set; }

    [Parameter]
    public bool Admin { get; set; }

    [Parameter]
    public DateTime ForDayLocal { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    private long disabledCount;
    private bool Disabled => Interlocked.Read(ref this.disabledCount) > 0;
    private PayPeriod MergedPayPeriod { get; set; }
    private PayPeriod UserPayPeriod { get; set; }
    private PayPeriod PayPeriodToUpdate => this.Admin ? this.MergedPayPeriod : this.UserPayPeriod;
    private PayPeriod PayPeriodToDisplay => this.MergedPayPeriod;

    protected override async Task OnParametersSetAsync()
    {
        await this.DisableDuring(async () =>
        {
            await this.SaveChanges();

            DateTime startDayLocal = this.User.TimeToPayPeriodStartLocal(this.ForDayLocal);
            NullableResponse<DataEntity> existingAdminEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.User.Partition, startDayLocal.DayToRowKey(admin: true));
            NullableResponse<DataEntity> existingUserEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.User.Partition, startDayLocal.DayToRowKey(admin: false));

            this.MergedPayPeriod = existingAdminEntity.HasValue
                ? existingAdminEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.User.PayPeriodType);

            this.UserPayPeriod = existingAdminEntity.HasValue
                ? existingUserEntity.Value.Deserialize<PayPeriod>()
                : PayPeriodUtility.NewPayPeriod(startDayLocal, this.User.PayPeriodType);

            this.MergedPayPeriod.Merge(this.UserPayPeriod);
        });
    }

    private async Task SaveChanges()
    {
        if (this.PayPeriodToUpdate != null)
        {
            await this.DisableDuring(async () =>
            {
                DataEntity entity = new DataEntity(this.PayPeriodToUpdate, this.User, this.Admin);
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

    private void NewTime(Day day)
    {
        day.Times.Add(new Time()
        {
            Type = TimeType.Work,
        });
    }

    private void DeleteTime(Day day, Time time)
    {
        day.Times.Remove(time);
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
