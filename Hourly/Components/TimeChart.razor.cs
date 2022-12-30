using Azure;
using Azure.Data.Tables;
using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class TimeChart : ComponentBase
{
    public PayPeriod PayPeriod { get; set; }
    public CancellationTokenSource CancelSource { get; set; }

    [Parameter]
    public User User { get; set; }

    [Parameter]
    public int PayPeriodIndex { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        this.PayPeriod = null;
        this.CancelSource?.Cancel();
        this.CancelSource = new();

        try
        {
            DateTime startDayLocal = this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex);
            string rowKey = startDayLocal.DayToPersistString();
            NullableResponse<DataEntity> existingEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.User.Partition, rowKey, cancellationToken: this.CancelSource.Token);

            if (existingEntity.HasValue)
            {
                this.PayPeriod = existingEntity.Value.Deserialize<PayPeriod>();
            }
            else
            {
                PayPeriod payPeriod = PayPeriodUtility.NewPayPeriod(startDayLocal, this.User.PayPeriodType);
                DataEntity newEntity = new(this.User.Partition, rowKey, this.PayPeriod);
                await this.TableClient.UpsertEntityAsync(newEntity, cancellationToken: this.CancelSource.Token);
                this.PayPeriod = payPeriod;
            }
        }
        catch (OperationCanceledException)
        {
            // that's cool
        }
        finally
        {
            this.CancelSource = null;
        }

        await base.OnParametersSetAsync();
    }
}
