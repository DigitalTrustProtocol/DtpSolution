using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using DtpGraphCore.Interfaces;
using DtpGraphCore.Model;
using DtpGraphCore.Services;
using DtpGraphCore.Model.Schema;
using MediatR;
using System.Reflection;

namespace DtpGraphCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpGraphCore(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton(new GraphModel());
            services.AddScoped<IDerivationStrategy, DerivationSecp256k1PKH>();
            services.AddScoped<IGraphLoadSaveService, GraphLoadSaveService>();
            services.AddScoped<IGraphClaimService, GraphClaimService>();
            services.AddTransient<IQueryRequestBinary, QueryRequestBinary>();

            services.AddTransient<IGraphQueryService, GraphQueryService>();
            services.AddTransient<IGraphWorkflowService, GraphWorkflowService>();
            services.AddTransient<IQueryRequestValidator, QueryRequestValidator>();
            
        }


    }
}
