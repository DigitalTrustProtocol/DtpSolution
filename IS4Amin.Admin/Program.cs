using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using IS4Amin.Admin.EntityFramework.Shared.DbContexts;
using IS4Amin.Admin.EntityFramework.Shared.Entities.Identity;
using IS4Amin.Admin.Helpers;

namespace IS4Amin.Admin
{
    public class Program
    {
        private const string SeedArgs = "/seed";

        public static async Task Main(string[] args)
        {
            var seed = args.Any(x => x == SeedArgs);
            if (seed) args = args.Except(new[] { SeedArgs }).ToArray();

            var host = BuildWebHost(args);

            // Uncomment this to seed upon startup, alternatively pass in `dotnet run /seed` to seed using CLI
            await DbMigrationHelpers.EnsureSeedData<IdentityServerConfigurationDbContext, AdminIdentityDbContext, IdentityServerPersistedGrantDbContext, AdminLogDbContext, UserIdentity, UserIdentityRole>(host);
            if (seed)
            {
                await DbMigrationHelpers.EnsureSeedData<IdentityServerConfigurationDbContext, AdminIdentityDbContext, IdentityServerPersistedGrantDbContext, AdminLogDbContext, UserIdentity, UserIdentityRole>(host);
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(c => c.AddServerHeader = false)
                   .UseUrls("http://localhost:9000")
                   .UseStartup<Startup>()
                   .UseSerilog()
                   .Build();
    }
}