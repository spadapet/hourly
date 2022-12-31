using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route($"/employee/{{{nameof(EmployeePage.UserId)}}}/{{{nameof(EmployeePage.UserPassword)}}}")]
public sealed partial class EmployeePage : TablePageBase
{
    public const string ViewDefault = EmployeePage.ViewPunch;
    public const string ViewPunch = "punch";
    public const string ViewChart = "chart";
    public const string ViewLoading = "loading";

    public string SelectedView { get; set; } = EmployeePage.ViewDefault;
    public DateTime ForDayLocal { get; set; } = DateTime.Now.Date;
}
