using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model.Schema;
using DtpPackageCore.Notifications;
using DtpPackageCore.Services;
using DtpPackageCore.Workflows;
using Ipfs.Http;
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
            
            services.AddTransient<IPackageService, PackageService>();

            services.AddTransient<BuildPackageCommandHandler>();
            services.AddTransient<CreateTrustPackageWorkflow>();

            services.AddTransient<IpfsClient>();


            services.AddTransient<IPackageMessageValidator, PackageMessageValidator>();
            

            services.AddSingleton<IPackageService, PackageService>();

        }
    }
}