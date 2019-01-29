using DtpCore.Notifications;
using DtpServer.AspNetCore;
using DtpServer.Notifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace DtpServer.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpServer(this IServiceCollection services)
        {
            services.AddMediatR(typeof(TrustPackageCreatedNotificationHandler));
            
            //services.AddTransient<BlockchainProofUpdatedNotificationHandler>();

            // https://github.com/Biarity/Sieve/issues/4#issuecomment-364629048
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();

        }
    }
}