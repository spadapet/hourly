using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route("/")]
public sealed partial class Index : ComponentBase
{
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    public string UserId { get; set; }
    public string Password { get; set; }

    private void Login()
    {
        this.NavigationManager.NavigateTo($"/employee/{this.UserId}/{this.Password}");
    }

    private void OnClickTestEmployeeHours()
    {
        this.NavigationManager.NavigateTo($"/employee/testUser/testPassword");
    }

    private void OnClickTestAdminHours()
    {
        this.NavigationManager.NavigateTo($"/employee/testUser/testAdminPassword");
    }
}
