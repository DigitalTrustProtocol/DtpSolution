using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using DtpCore.Services;
using DtpStampCore.Workflows;
using DtpPackageCore.Workflows;
using DtpPackageCore.Interfaces;
using DtpGraphCore.Interfaces;
using DtpCore.Service;

namespace DtpServer.Extensions
{
    public static class IApplicationBuilderExtensions
    {

        //public static void StartIPFS(this IApplicationBuilder app)
        //{
        //    var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();
        //    using (var scope = app.ApplicationServices.CreateScope())
        //    {
        //        var ipfsManager = scope.ServiceProvider.GetRequiredService<IpfsManager>();
        //        ipfsManager.StartIpfs();
        //        applicationEvent.StopTasks.Add(async () => await Task.Run(() => ipfsManager.Dispose()));
        //    }
        //}

        public static void DtpGraph(this IApplicationBuilder app)
        {
            var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();
            applicationEvent.BootupTasks.Add(Task.Run(() => {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    using (TimeMe.Track("Trust load"))
                    {

                        // Load all data into graph, properly async will be an good idea here!
                        var trustLoadService = scope.ServiceProvider.GetRequiredService<IGraphLoadSaveService>();
                        trustLoadService.LoadFromDatabase();
                    }
                }
            }));
        }

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
                    //workflowService.EnsureWorkflow<SynchronizePackageWorkflow>();

                    //var packageService = scope.ServiceProvider.GetRequiredService<IPackageService>();
                    //packageService.AddPackageSubscriptions();
                }
            }));
        }

        public static void DtpStamp(this IApplicationBuilder app)
        {
            var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();

            applicationEvent.BootupTasks.Add(Task.Run(() =>
            {

                // Ensure that a Timestamp workflow is running.
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    //var timestampWorkflowService = scope.ServiceProvider.GetRequiredService<ITimestampWorkflowService>();
                    //timestampWorkflowService.EnsureTimestampScheduleWorkflow();
                    //timestampWorkflowService.CreateAndExecute(); // Make sure that there is a Timestamp engine workflow
                    var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                    workflowService.EnsureWorkflow<CreateProofWorkflow>();
                    workflowService.EnsureWorkflow<UpdateProofWorkflow>();

                }
            }));
        }
    }
}
