using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpCore.Interfaces;

namespace DtpCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpCore(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Load all data into graph, properly async will be an good idea here!
                var trustDBService  = scope.ServiceProvider.GetRequiredService<ITrustDBService>();
                trustDBService.EnsureBuildPackage();
            }
        }
    }
}
