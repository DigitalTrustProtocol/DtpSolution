using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Notifications;
using DtpPackageCore.Services;
using DtpPackageCore.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DtpPackageCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpPackageCore(this IServiceCollection services)
        {
            services.AddMediatR(typeof(BuildPackageCommandHandler));
//            services.AddMediatR(typeof(TrustPackageCreatedNotificationHandler));
            
            services.AddTransient<ITrustPackageService, TrustPackageService>();

            services.AddTransient<BuildPackageCommandHandler>();
            services.AddTransient<CreateTrustPackageWorkflow>();
            

        }
    }
}