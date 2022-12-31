using Azure.Data.Tables;
using Hourly.Data;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

public abstract class TablePageBase : ComponentBase
{
    public User User { get; private set; }
    public bool Admin { get; private set; }

    [Parameter]
    public string UserId { get; set; }

    [Parameter]
    public string UserPassword { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    [Inject]
    private Users Users { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        this.User = null;
        this.Admin = false;

        if (this.Users.ById.TryGetValue(this.UserId, out User user) && (this.UserPassword == user.Password || this.UserPassword == user.AdminPassword))
        {
            this.User = user;
            this.Admin = (this.UserPassword == user.AdminPassword);
        }
        else
        {
            throw new InvalidOperationException($"Unknown user: {this.UserId}");
        }

        await base.OnParametersSetAsync();
    }
}
