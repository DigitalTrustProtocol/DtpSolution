﻿using Microsoft.Extensions.DependencyInjection;
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
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var applicationEvent = scope.ServiceProvider.GetRequiredService<ApplicationEvents>();

                var ipfsManager = scope.ServiceProvider.GetRequiredService<IpfsManager>();
                ipfsManager.StartIpfs();
                applicationEvent.StopTasks.Add(async () => await Task.Run(() => ipfsManager.Dispose()));
            }
        }
    }
}
