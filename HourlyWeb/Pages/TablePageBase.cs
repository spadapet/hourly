using Azure.Data.Tables;
using Hourly.Data;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

public abstract class TablePageBase : ComponentBase
{
    [Parameter]
    public string UserId { get; set; }

    [Parameter]
    public string UserPassword { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    [Inject]
    private Users Users { get; set; }

    public ViewModel ViewModel { get; }

    protected TablePageBase()
    {
        this.ViewModel = new(this.StateHasChanged);
    }

    protected override async Task OnParametersSetAsync()
    {
        this.ViewModel.User = null;
        this.ViewModel.Admin = false;

        if (this.Users.ById.TryGetValue(this.UserId, out User user) && (this.UserPassword == user.Password || this.UserPassword == user.AdminPassword))
        {
            this.ViewModel.User = user;
            this.ViewModel.Admin = (this.UserPassword == user.AdminPassword);
        }
        else
        {
            throw new InvalidOperationException($"Unknown user: {this.UserId}");
        }

        await base.OnParametersSetAsync();
    }
}
