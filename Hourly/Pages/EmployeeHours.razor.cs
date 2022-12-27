using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Principal;

namespace Hourly.Pages
{
    [Route("/employee")]
    public partial class EmployeeHours : ComponentBase
    {
        public bool authenticated;
        public string name;
        public string authType;

        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject]
        public TableServiceClient TableServiceClient { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState user = await this.AuthenticationStateTask;

            if (user?.User?.Identity is IIdentity identity)
            {
                this.authenticated = identity.IsAuthenticated;
                this.name = identity.Name;
                this.authType = identity.AuthenticationType;
            }

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //string name = string.Empty;
            //
            //try
            //{
            //    TableClient table = TableServiceClient.GetTableClient("TestHourly");
            //    var row = await table.GetEntityAsync<TableEntity>("a", "b");
            //
            //    name = (string)row.Value["Name"];
            //}
            //catch (Exception ex)
            //{
            //    name = ex.ToString();
            //}

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
