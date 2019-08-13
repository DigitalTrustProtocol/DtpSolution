using DtpServer.AspNetCore;
//using DtpServer.Platform;
//using DtpServer.Platform.ipfs;
//using DtpServer.Platform.IPFS;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace DtpServer.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpServer(this IServiceCollection services)
        {
            // https://github.com/Biarity/Sieve/issues/4#issuecomment-364629048
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
            //services.AddTransient<PlatformDirectory>();
            //services.AddSingleton<IPFSShell>();
        }
    }
}