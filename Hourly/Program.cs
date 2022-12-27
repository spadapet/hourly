using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

internal static class Program
{
    public static IReadOnlyDictionary<string, string> NameToTable = new Dictionary<string, string>();

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthentication().AddOAuth()
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor()
            .AddMicrosoftIdentityConsentHandler();

        if (builder.Configuration["BlobServer"] is string blobServer && blobServer.Length != 0)
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddTableServiceClient(new Uri(blobServer));
                clientBuilder.UseCredential(new DefaultAzureCredential());
            });
        }

        builder.Configuration.Bind("EmployeeTables", Program.NameToTable);

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        app.Run();
    }
}
