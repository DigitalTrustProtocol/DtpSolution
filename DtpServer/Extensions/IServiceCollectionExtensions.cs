using DtpCore.Notifications;
using DtpServer.Notifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DtpServer.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpServer(this IServiceCollection services)
        {
            services.AddMediatR(typeof(TrustPackageCreatedNotificationHandler));
            
            services.AddTransient<BlockchainProofUpdatedNotificationHandler>();

        }
    }
}