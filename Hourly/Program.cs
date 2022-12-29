using Azure.Data.Tables;
using Azure.Identity;
using Hourly.Data;

namespace Hourly;

public static class Program
{
    public static bool IsDevelopment { get; private set; }
    public static TimeZoneInfo TimeZone { get; private set; }

    private static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        Settings settings = builder.Configuration.GetRequiredSection(nameof(Settings)).Get<Settings>();
        TableServiceClient tableServiceClient = new(new Uri(settings.Server), new DefaultAzureCredential());
        TableClient tableClient = tableServiceClient.GetTableClient(settings.Table);

        Program.IsDevelopment = builder.Environment.IsDevelopment();
        Program.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton(tableClient);
        builder.Services.AddSingleton(await Program.GetUsersAsync(tableClient));

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        await app.RunAsync();
    }

    private static async Task<Users> GetUsersAsync(TableClient tableClient)
    {
        DataEntity usersEntity = await tableClient.GetEntityAsync<DataEntity>("0", "Users");
        Users users = usersEntity.Deserialize<Users>();

        foreach (KeyValuePair<string, User> kvp in users.ById)
        {
            kvp.Value.Id = kvp.Key;
        }

        foreach (var user in users.ById.Values.ToArray())
        {
            if (user.DevOnly && !Program.IsDevelopment)
            {
                users.ById.Remove(user.Id);
            }
        }

        return users;
    }
}
