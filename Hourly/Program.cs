using Azure.Identity;
using Microsoft.Extensions.Azure;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        //string tableConnectionString = builder.Configuration.GetConnectionString("Table") ?? throw new InvalidOperationException("'Table' connection string missing");
        builder.Services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddTableServiceClient(new Uri("https://spadapet.table.core.windows.net/"));
            clientBuilder.UseCredential(new DefaultAzureCredential());
        });

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        app.Run();
    }
}
