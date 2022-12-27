using Microsoft.AspNetCore.Components;

namespace Hourly.Pages
{
    [Route("/")]
    public partial class Index : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private void OnClickEmployeeHours()
        {
            this.NavigationManager.NavigateTo("/employee");
        }
    }
}
