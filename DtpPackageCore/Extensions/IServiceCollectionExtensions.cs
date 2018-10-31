using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DtpPackageCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpPackageCore(this IServiceCollection services)
        {
            services.AddMediatR(typeof(TrustPackageCommandHandler));
            //services.AddMediatR(typeof(UpdateTrustPackageCommandHandler));

            services.AddTransient<ITrustPackageService, TrustPackageService>();

            services.AddTransient<TrustPackageCommandHandler>();
            //services.AddTransient<UpdateTrustPackageCommandHandler>();

        }
    }
}