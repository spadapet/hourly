using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route($"/employee/{{{nameof(EmployeePage.UserId)}}}/{{{nameof(EmployeePage.UserPassword)}}}")]
public sealed partial class EmployeePage : TablePageBase
{
    private async Task PunchClock()
    {
        if (this.ViewModel.PunchClockAction != null)
        {
            await this.ViewModel.PunchClockAction();
        }
    }
}
