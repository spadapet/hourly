using Microsoft.AspNetCore.Components;

namespace Hourly.Pages
{
    [Route("/")]
    public partial class Index : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private void OnClickTestEmployeeHours()
        {
            this.NavigationManager.NavigateTo($"/employee/{Program.TestUser}/{Program.TestKey}");
        }
    }
}
