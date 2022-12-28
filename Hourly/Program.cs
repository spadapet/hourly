using Azure.Identity;
using Microsoft.Extensions.Azure;

namespace Hourly
{
    internal static class Program
    {
        public struct TableEntry
        {
            public string TableName { get; }
            public string UserKey { get; }
            public bool ReadOnly { get; }

            public TableEntry(string tableName, string userKey, bool readOnly = false)
            {
                this.TableName = tableName;
                this.UserKey = userKey;
                this.ReadOnly = readOnly;
            }
        }

        private static Dictionary<string, TableEntry> nameToTable = new();
        public static IReadOnlyDictionary<string, TableEntry> NameToTable => Program.nameToTable;
        public static string TestUser => "testUser";
        public static string TestKey => "testKey";
        public static string TestTable => "TestHours";

        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            if (builder.Configuration["BlobServer"] is string blobServer)
            {
                builder.Services.AddAzureClients(clientBuilder =>
                {
                    clientBuilder.AddTableServiceClient(new Uri(blobServer));
                    clientBuilder.UseCredential(new DefaultAzureCredential());
                });
            }

            if (builder.Configuration["TableNames"] is string tableEntries)
            {
                foreach (string tableEntry in tableEntries.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    if (tableEntry.Split(',') is string[] datas && datas.Length == 3 &&
                        datas[0] is string userName && !string.IsNullOrEmpty(userName) &&
                        datas[1] is string tableName && !string.IsNullOrEmpty(tableName) &&
                        datas[2] is string userKey && !string.IsNullOrEmpty(userKey))
                    {
                        Program.nameToTable[userName] = new Program.TableEntry(tableName, userKey);
                    }
                }
            }

            if (builder.Environment.IsDevelopment())
            {
                Program.nameToTable[Program.TestUser] = new Program.TableEntry(Program.TestTable, Program.TestKey);
            }

            WebApplication app = builder.Build();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.Run();
        }
    }
}
