using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpGraph(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Load all data into graph, properly async will be an good idea here!
                var trustLoadService = scope.ServiceProvider.GetRequiredService<IGraphLoadSaveService>();
                trustLoadService.LoadFromDatabase();

                // Activate and expire
                //var graphWorkflowService = scope.ServiceProvider.GetRequiredService<IGraphWorkflowService>();
            }
        }
    }
}
