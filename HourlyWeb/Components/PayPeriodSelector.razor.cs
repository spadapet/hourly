using Hourly.Data;
using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Components;

public sealed partial class PayPeriodSelector : ComponentBase
{
    [Parameter]
    public ViewModel ViewModel { get; set; }

    private int PayPeriodIndex => this.ViewModel.User.PayPeriodIndex(this.ViewModel.ForDayLocal);
    private string PayPeriodStartString => this.ViewModel.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex).DayToNoYearDisplayString();
    private string PayPeriodLastString => this.ViewModel.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex + 1).AddDays(-1).DayToDisplayString();
    private bool HasPrevPayPeriod => this.PayPeriodIndex > 0;
    private bool HasNextPayPeriod => this.PayPeriodIndex >= 0;

    private void PrevPayPeriod()
    {
        if (this.HasPrevPayPeriod)
        {
            this.ViewModel.ForDayLocal = this.ViewModel.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex - 1);
        }
    }

    private void NextPayPeriod()
    {
        if (this.HasNextPayPeriod)
        {
            this.ViewModel.ForDayLocal = this.ViewModel.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex + 1);
        }
    }

    private void ThisPayPeriod()
    {
        this.ViewModel.ForDayLocal = TimeUtility.LocalDate;
    }
}
