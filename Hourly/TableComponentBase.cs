using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;

namespace Hourly
{
    public abstract class TableComponentBase : ComponentBase
    {
        public string TableName { get; private set; }

        [Parameter]
        public string UserName { get; set; }

        [Parameter]
        public string UserKey { get; set; }

        [Inject]
        public TableServiceClient TableServiceClient { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            this.TableName = null;

            if (!string.IsNullOrEmpty(this.UserName) &&
                !string.IsNullOrEmpty(this.UserKey) &&
                Program.NameToTable.TryGetValue(this.UserName, out Program.TableEntry entry))
            {
                if (this.UserKey == entry.UserKey)
                {
                    this.TableName = entry.TableName;
                }
            }

            await base.OnParametersSetAsync();
        }
    }
}
