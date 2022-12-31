using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

[Route("/")]
public sealed partial class Index : ComponentBase
{
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    private void OnClickTestEmployeeHours()
    {
        this.NavigationManager.NavigateTo($"/employee/testUser/testPassword");
    }
}
