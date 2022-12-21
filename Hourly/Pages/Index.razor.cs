using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages
{
    [Route("/")]
    public partial class Index : ComponentBase
    {
        private bool clicked = false;
        private string name = "Loading...";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            string name = string.Empty;

            try
            {
                TableServiceClient client = new TableServiceClient(new Uri("https://spadapet.table.core.windows.net/"), new DefaultAzureCredential());
                TableClient table = client.GetTableClient("TestHourly");
                var row = await table.GetEntityAsync<TableEntity>("a", "b");

                name = (string)row.Value["Name"];
            }
            catch (Exception ex)
            {
                name = ex.ToString();
            }

            if (!string.Equals(this.name, name))
            {
                this.name = name;
                this.StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private void ButtonClicked()
        {
            this.clicked = true;
        }
    }
}
