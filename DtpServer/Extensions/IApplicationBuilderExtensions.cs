using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpServer.Platform.IPFS;
using System.Threading.Tasks;
using DtpCore.Services;

namespace DtpServer.Extensions
{
    public static class IApplicationBuilderExtensions
    {

        public static void DtpServer(this IApplicationBuilder app)
        {
            var applicationEvent = app.ApplicationServices.GetRequiredService<ApplicationEvents>();
            applicationEvent.BootupTasks.Add(Task.Run(() =>
            {

                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var ipfsManager = scope.ServiceProvider.GetRequiredService<IpfsManager>();
                    ipfsManager.StartIpfs();
                    applicationEvent.StopTasks.Add(async () => await Task.Run(() => ipfsManager.Dispose()));
                }
            }));
        }
    }
}
