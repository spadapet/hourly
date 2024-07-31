using Azure.Data.Tables;
using Azure.Identity;
using Hourly.Data;
using Hourly.Utility;

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
        builder.Services.AddSingleton(await UserUtility.GetUsersAsync(tableClient));

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        await app.RunAsync();
    }
}
