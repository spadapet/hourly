using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route("/employee/{UserName}/{UserPassword}")]
public sealed partial class EmployeeHours : TablePageBase
{
    public int PayPeriodIndex { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        this.PayPeriodIndex = TimeUtility.CurrentPayPeriodIndex(this.User.PayPeriodType, this.User.FirstWorkDay);
    }

    public string PayPeriodStartString => TimeUtility.DayToString(TimeUtility.IndexToPayPeriodStart(this.PayPeriodIndex, this.User.PayPeriodType, this.User.FirstWorkDay));
    public string PayPeriodLastString => TimeUtility.DayToString(TimeUtility.IndexToPayPeriodStart(this.PayPeriodIndex + 1, this.User.PayPeriodType, this.User.FirstWorkDay).AddDays(-1));
}
