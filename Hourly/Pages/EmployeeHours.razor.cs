using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages
{
    [Route("/employee/{UserName}/{UserKey}")]
    public partial class EmployeeHours : TableComponentBase
    {
        protected override async Task OnInitializedAsync()
        {
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
