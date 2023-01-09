using Azure;
using Azure.Data.Tables;
using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;

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
            this.MergedPayPeriod = await this.FetchPayPeriod(startDayLocal, admin: true);
            this.UserPayPeriod = await this.FetchPayPeriod(startDayLocal, admin: false);
            this.MergedPayPeriod.Merge(this.UserPayPeriod);
        }, runIfDisabled: false);

        await base.OnParametersSetAsync();
    }

    private async Task<PayPeriod> FetchPayPeriod(DateTime startDayLocal, bool admin)
    {
        NullableResponse<DataEntity> entity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.ViewModel.User.Partition, startDayLocal.DayToRowKey(admin));

        PayPeriod payPeriod = entity.HasValue
            ? entity.Value.Deserialize<PayPeriod>().Canonicalize()
            : PayPeriodUtility.NewPayPeriod(startDayLocal, this.ViewModel.User.PayPeriodType, this.ViewModel.User.PayRate);

        return payPeriod;
    }

    private async Task SaveChanges()
    {
        if (this.ViewModel.Admin && this.MergedPayPeriod is PayPeriod payPeriodToUpdate)
        {
            await this.SaveChanges(payPeriodToUpdate, admin: true);
        }
    }

    private async Task SaveChanges(PayPeriod payPeriod, bool admin)
    {
        payPeriod = payPeriod.Canonicalize();

        await this.DisableDuring(async () =>
        {
            DataEntity entity = new(payPeriod, this.ViewModel.User, admin);
            Response response = await this.TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
            if (response.IsError)
            {
                throw new InvalidOperationException($"Failed to save entity: {entity.PartitionKey}, {entity.RowKey}");
            }
        });
    }

    private async Task ResetChanges()
    {
        this.MergedPayPeriod = null;
        this.UserPayPeriod = null;
        await this.OnParametersSetAsync();
    }

    private static void NewTime(Day day)
    {
        day.Times.Add(new Time()
        {
            Type = TimeType.Work,
            StartLocal = TimeUtility.LocalNow.MoveTimeToDay(day.DayLocal),
        });
    }

    private void DeleteTime(Day day, Time time)
    {
        if (this.UserPayPeriod?.Days.FirstOrDefault(d => d.DayLocal == day.DayLocal) is Day userDay && userDay.Times.Contains(time))
        {
            // The employee created this work time, so it should be marked as deleted rather than actually deleted.
            // Otherwise it's just going to show up again.
            time.Type = TimeType.Deleted;
        }
        else
        {
            day.Times.Remove(time);
        }
    }

    private async Task PunchClock()
    {
        DateTime punchTime = TimeUtility.LocalNow;

        await this.DisableDuring(async () =>
        {
            DateTime startDayLocal = this.ViewModel.User.TimeToPayPeriodStartLocal(punchTime);
            PayPeriod payPeriod = await this.FetchPayPeriod(startDayLocal, admin: false);
            payPeriod.PunchClock(punchTime);

            await this.SaveChanges(payPeriod, admin: false);
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

    private async Task DisableDuring(Func<Task> taskFunc, bool runIfDisabled = true)
    {
        if (!this.Disabled || runIfDisabled)
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
}
