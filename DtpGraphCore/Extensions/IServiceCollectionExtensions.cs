using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using DtpGraphCore.Interfaces;
using DtpGraphCore.Model;
using DtpGraphCore.Services;
using DtpGraphCore.Workflows;

namespace DtpGraphCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpGraphCore(this IServiceCollection services)
        {
            services.AddSingleton(new GraphModel());
            services.AddScoped<IDerivationStrategy, DerivationBTCPKH>();
            services.AddScoped<IGraphLoadSaveService, GraphLoadSaveService>();
            services.AddScoped<IGraphTrustService, GraphTrustService>();

            services.AddTransient<IGraphQueryService, GraphQueryService>();
            services.AddTransient<IQueryRequestService, QueryRequestService>();
            services.AddTransient<IGraphWorkflowService, GraphWorkflowService>();

            services.AddTransient<TrustPackageWorkflow>();

            //services.AddTransient<ITrustTimestampStep, TrustTimestampStep>();
            //services.AddTransient<TrustTimestampWorkflow>();


        }

    }
}
