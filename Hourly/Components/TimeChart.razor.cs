﻿using Azure;
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

    private PayPeriod PayPeriod { get; set; }
    private CancellationTokenSource CancelSource { get; set; }
    private Dictionary<string, PayPeriod> payPeriods = new();

    protected override async Task OnParametersSetAsync()
    {
        this.PayPeriod = null;
        this.CancelSource?.Cancel();
        this.CancelSource = new();

        try
        {
            DateTime startDayLocal = this.User.TimeToPayPeriodStartLocal(this.ForDayLocal);
            string rowKey = startDayLocal.DayToPersistString();

            if (this.payPeriods.TryGetValue(rowKey, out PayPeriod payPeriod))
            {
                this.PayPeriod = payPeriod;
            }
            else
            {
                NullableResponse<DataEntity> existingEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.User.Partition, rowKey, cancellationToken: this.CancelSource.Token);

                if (existingEntity.HasValue)
                {
                    this.PayPeriod = existingEntity.Value.Deserialize<PayPeriod>();
                }
                else
                {
                    payPeriod = PayPeriodUtility.NewPayPeriod(startDayLocal, this.User.PayPeriodType);
                    DataEntity newEntity = new(this.User.Partition, rowKey, payPeriod);
                    await this.TableClient.UpsertEntityAsync(newEntity, cancellationToken: this.CancelSource.Token);
                    this.PayPeriod = payPeriod;
                }

                this.payPeriods[rowKey] = this.PayPeriod;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignored
        }
        finally
        {
            this.CancelSource = null;
        }

        await base.OnParametersSetAsync();
    }

    private void NewTime(Day day)
    {
        day.Times.Add(new Time()
        {
            Type = TimeType.Work,
        });
    }
}
