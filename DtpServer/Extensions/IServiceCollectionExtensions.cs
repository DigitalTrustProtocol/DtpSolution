using DtpServer.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sieve.Services;
using System.Reflection;

namespace DtpServer.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpServer(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<MvcNewtonsoftJsonOptions>, JsonOptionsSetup>();
            services.AddMediatR(Assembly.GetExecutingAssembly());

            // https://github.com/Biarity/Sieve/issues/4#issuecomment-364629048
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
            //services.AddTransient<PlatformDirectory>();
            //services.AddSingleton<IPFSShell>();
        }
    }
}