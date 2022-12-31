using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class PayPeriodSelector : ComponentBase
{
    [Parameter]
    public User User { get; set; }

    [Parameter]
    public DateTime ForDayLocal { get; set; }

    [Parameter]
    public EventCallback<DateTime> ForDayLocalChanged { get; set; }

    private int PayPeriodIndex => this.User.PayPeriodIndex(this.ForDayLocal);
    private string PayPeriodStartString => this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex).DayToNoYearDisplayString();
    private string PayPeriodLastString => this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex + 1).AddDays(-1).DayToDisplayString();
    private bool HasPrevPayPeriod => this.PayPeriodIndex > 0;
    private bool HasNextPayPeriod => this.PayPeriodIndex <= this.User.CurrentPayPeriodIndex() + 8;

    private async Task PrevPayPeriod()
    {
        if (this.HasPrevPayPeriod)
        {
            this.ForDayLocal = this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex - 1);
            await this.ForDayLocalChanged.InvokeAsync(this.ForDayLocal);
        }
    }

    private async Task NextPayPeriod()
    {
        if (this.HasNextPayPeriod)
        {
            this.ForDayLocal = this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex + 1);
            await this.ForDayLocalChanged.InvokeAsync(this.ForDayLocal);
        }
    }
}
