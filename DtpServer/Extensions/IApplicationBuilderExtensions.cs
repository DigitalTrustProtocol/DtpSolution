using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpServer.Platform.IPFS;
using System.Threading.Tasks;

namespace DtpServer.Extensions
{
    public static class IApplicationBuilderExtensions
    {

        public static void DtpServer(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var ipfsManager = scope.ServiceProvider.GetRequiredService<IpfsManager>();
                ipfsManager.StartIpfs();
                Startup.StopTasks.Add(async () => await Task.Run(() => ipfsManager.Dispose()));
                //Startup.StopTasks.Add(() =>
                //    {
                //        ipfsManager.Dispose();
                //        return Task.CompletedTask;
                //    }
                //);
            }
        }
    }
}
