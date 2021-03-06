﻿using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model.Schema;
using DtpPackageCore.Services;
using DtpPackageCore.Workflows;
using Ipfs.CoreApi;
using Ipfs.Http;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DtpPackageCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpPackageCore(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton<PubSubMessageController>();

            services.AddTransient<BuildPackageCommandHandler>();
            services.AddTransient<CreateTrustPackageWorkflow>();
            services.AddTransient<SynchronizePackageWorkflow>();

            services.AddSingleton<IPackageMessageValidator, PackageMessageValidator>();
            services.AddScoped<ICoreApi, IpfsClient>();
            services.AddScoped<IPackageService, IpfsPackageService>();
        }
    }
}