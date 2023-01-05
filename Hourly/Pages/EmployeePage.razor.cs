using Hourly.Utility;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route($"/employee/{{{nameof(EmployeePage.UserId)}}}/{{{nameof(EmployeePage.UserPassword)}}}")]
public sealed partial class EmployeePage : TablePageBase
{
    public DateTime ForDayLocal { get; set; } = TimeUtility.LocalNow;
}
