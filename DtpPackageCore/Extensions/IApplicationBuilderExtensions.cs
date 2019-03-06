using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpCore.Services;
using DtpPackageCore.Workflows;
using DtpPackageCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DtpPackageCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpPackage(this IApplicationBuilder app)
        {
            var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();
            applicationEvent.BootupTasks.Add(Task.Run(() =>
            {
                // Ensure that workflows are installed.
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                    workflowService.EnsureWorkflow<CreateTrustPackageWorkflow>();
                    workflowService.EnsureWorkflow<SynchronizePackageWorkflow>();

                    var packageService = scope.ServiceProvider.GetRequiredService<IPackageService>();
                    packageService.AddPackageSubscriptions();
                }
            }));
        }
    }
}
