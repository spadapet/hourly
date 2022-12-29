using Azure;
using Azure.Data.Tables;
using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class TimeChart : ComponentBase
{
    public PayPeriod PayPeriod { get; set; }

    [Parameter]
    public User User { get; set; }

    [Parameter]
    public int PayPeriodIndex { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        DateTime startDay = TimeUtility.IndexToPayPeriodStart(this.PayPeriodIndex, this.User.PayPeriodType, this.User.FirstWorkDay);
        NullableResponse<DataEntity> dataEntity = await this.TableClient.GetEntityIfExistsAsync<DataEntity>(this.User.Partition, TimeUtility.DayToString(startDay));
        PayPeriod period = null;

        if (dataEntity.HasValue)
        {
            period = dataEntity.Value.Deserialize<PayPeriod>();
        }
        else
        {
            period = new PayPeriod()
            {
            };
        }

        await base.OnParametersSetAsync();
    }
}
