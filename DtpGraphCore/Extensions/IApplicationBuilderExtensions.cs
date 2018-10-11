using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpGraphCore.Interfaces;
using DtpCore.Services;
using DtpGraphCore.Workflows;

namespace DtpGraphCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void Graph(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Load all data into graph, properly async will be an good idea here!
                var trustLoadService = scope.ServiceProvider.GetRequiredService<IGraphLoadSaveService>();
                trustLoadService.LoadFromDatabase();

                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                workflowService.EnsureWorkflow<TrustPackageWorkflow>();

                // Activate and expire
                //var graphWorkflowService = scope.ServiceProvider.GetRequiredService<IGraphWorkflowService>();
            }
        }
    }
}
