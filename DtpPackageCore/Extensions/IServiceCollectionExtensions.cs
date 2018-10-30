using DtpPackageCore.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DtpPackageCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpPackageCore(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateTrustPackageCommandHandler));
            services.AddMediatR(typeof(TimestampTrustPackageCommandHandler));

            services.AddTransient<CreateTrustPackageCommandHandler>();
            services.AddTransient<TimestampTrustPackageCommandHandler>();

        }
    }
}