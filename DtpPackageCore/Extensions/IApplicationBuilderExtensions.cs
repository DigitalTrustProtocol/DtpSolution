using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpCore.Services;
using DtpPackageCore.Workflows;
using DtpPackageCore.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DtpPackageCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpPackage(this IApplicationBuilder app)
        {
            // Ensure that workflows are installed.
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                workflowService.EnsureWorkflow<CreateTrustPackageWorkflow>();

                var packageService = scope.ServiceProvider.GetRequiredService<IPackageService>();
                packageService.AddPackageSubscriptionsAsync();
            }
        }
    }
}
