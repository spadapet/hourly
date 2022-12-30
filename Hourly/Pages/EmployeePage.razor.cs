using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route("/employee/{UserName}/{UserPassword}")]
public sealed partial class EmployeePage : TablePageBase
{
    public const string ViewDefault = EmployeePage.ViewPunch;
    public const string ViewPunch = "punch";
    public const string ViewSick = "sick";
    public const string ViewHoliday = "holiday";
    public const string ViewVacation = "vacation";
    public const string ViewTimeSheet = "sheet";

    public string SelectedView { get; set; } = EmployeePage.ViewDefault;
    public int PayPeriodIndex { get; set; }
    public string PayPeriodStartString => this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex).DayToDisplayString();
    public string PayPeriodLastString => this.User.IndexToPayPeriodStartLocal(this.PayPeriodIndex + 1).AddDays(-1).DayToDisplayString();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        this.PayPeriodIndex = this.User.CurrentPayPeriodIndex();
    }

    private bool HasPrevPayPeriod => this.PayPeriodIndex > 0;
    private bool HasNextPayPeriod => this.PayPeriodIndex <= this.User.CurrentPayPeriodIndex() + 8;
    private bool CanResetPayPeriod => this.PayPeriodIndex != this.User.CurrentPayPeriodIndex();

    private void PrevPayPeriod()
    {
        if (this.HasPrevPayPeriod)
        {
            this.PayPeriodIndex--;
        }
    }

    private void NextPayPeriod()
    {
        if (this.HasNextPayPeriod)
        {
            this.PayPeriodIndex++;
        }
    }

    private void ResetPayPeriod()
    {
        this.PayPeriodIndex = this.User.CurrentPayPeriodIndex();
    }
}
