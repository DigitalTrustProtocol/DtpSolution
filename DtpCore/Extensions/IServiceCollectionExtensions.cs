using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using DtpCore.Factories;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Services;
using DtpCore.Strategy;
using DtpCore.Workflows;
using DtpCore.Strategy.Serialization;
using DtpCore.Builders;
using DtpCore.Notifications;
using DtpCore.Model.Schema;

namespace DtpCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpCore(this IServiceCollection services)
        {

            services.AddSingleton<ApplicationEvents>();
            services.AddScoped<NotificationSegment>();
            
            services.AddScoped<IDTPApiService, DTPApiService>();
            services.AddScoped<IClaimBinary, ClaimBinary>();
            services.AddScoped<IPackageBinary, PackageBinary>();
            services.AddScoped<ITrustDBService, TrustDBService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IKeyValueService, KeyValueService>();
            services.AddScoped<IClaimBanListService, ClaimBanListService>();

            services.AddTransient<PackageBuilder>();
            services.AddTransient<NumericIdentityValidator>();
            services.AddTransient<AlphaNumericIdentityValidator>();
            services.AddTransient<Secp256k1PKHIdentityValidator>();
            services.AddTransient<DTPAddressIdentityValidator>();
            services.AddTransient<UriIdentityValidator>();
            services.AddTransient<StringIdentityValidator>();

            //services.AddTransient<ISecp256k1PKHIdentityValidator, Secp256k1PKHIdentityValidator>();
            //services.AddTransient<IAddressIdentityValidator, DTPAddressIdentityValidator>();

            services.AddTransient<IValidatorFactory, ValidatorFactory>();
            services.AddTransient<IPackageSchemaValidator, PackageSchemaValidator>();

            services.AddTransient<IHashAlgorithmFactory, HashAlgorithmFactory>();
            services.AddTransient<IMerkleStrategyFactory, MerkleStrategyFactory>();
            services.AddTransient<IDerivationStrategyFactory, DerivationStrategyFactory>();
            services.AddTransient<DerivationSecp256k1PKH>();
            services.AddTransient<IServerIdentityService, ServerIdentityService>();

            // ---------------------------------------------------------------------------------------------------------------
            // http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
            services.AddSingleton<IDIMeta>(s => { return new DIMetaDefault(services); });
            services.AddSingleton<IDIReverseMeta>(s => { return new DIMetaReverseDefault(services); });
            services.AddTransient<IContractResolver, DIContractResolver>();
            services.AddTransient<IContractReverseResolver, DIContractReverseResolver>();
            
            services.AddTransient<IConfigureOptions<MvcJsonOptions>, JsonOptionsSetup>();
            services.AddTransient<IWorkflowContext, WorkflowContext>();

            services.AddTransient<WorkflowContainer>();

            services.AddTransient<PackageBuilder>();

            

            // ---------------------------------------------------------------------------------------------------------------
        }

    }
}
