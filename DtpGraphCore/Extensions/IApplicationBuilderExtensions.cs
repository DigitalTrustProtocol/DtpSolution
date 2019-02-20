using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpGraphCore.Interfaces;
using DtpCore.Services;
using System.Threading.Tasks;

namespace DtpGraphCore.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void DtpGraph(this IApplicationBuilder app)
        {
            var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();

            applicationEvent.BootupTasks.Add(Task.Run(() => { 
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    // Load all data into graph, properly async will be an good idea here!
                    var trustLoadService = scope.ServiceProvider.GetRequiredService<IGraphLoadSaveService>();
                    trustLoadService.LoadFromDatabase();
                }
            }));
        }
    }
}
