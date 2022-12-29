using Azure.Data.Tables;
using Hourly.Data;
using Microsoft.AspNetCore.Components;

namespace Hourly.Pages;

public abstract class TablePageBase : ComponentBase
{
    public User User { get; private set; }

    [Parameter]
    public string UserName { get; set; }

    [Parameter]
    public string UserPassword { get; set; }

    [Inject]
    public TableClient TableClient { get; set; }

    [Inject]
    private Users Users { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        User = null;

        if (Users.ById.TryGetValue(UserName, out User user) && UserPassword == user.Password)
        {
            User = user;
        }
        else
        {
            throw new InvalidOperationException($"Unknown user: {UserName}");
        }

        await base.OnParametersSetAsync();
    }
}
