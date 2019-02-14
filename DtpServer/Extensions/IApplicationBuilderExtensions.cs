using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DtpCore.Services;
using DtpPackageCore.Workflows;
using DtpPackageCore.Interfaces;
using Microsoft.Extensions.Configuration;
using DtpServer.Platform.IPFS;

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

            }
        }

        public static void DtpServerDispose(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var ipfsManager = scope.ServiceProvider.GetRequiredService<IpfsManager>();
                ipfsManager.Dispose(); ;

            }
        }

    }
}
