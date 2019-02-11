using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using DtpStampCore.Factories;
using DtpStampCore.Interfaces;
using DtpStampCore.Repository;
using DtpStampCore.Services;
using DtpStampCore.Workflows;
using DtpStampCore.Model.Schema;

namespace DtpStampCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void DtpStrampCore(this IServiceCollection services)
        {
            services.AddTransient<IHashAlgorithm, Double256>();
            services.AddTransient<IMerkleTree, MerkleTreeSorted>();

            services.AddTransient<CreateProofWorkflow>();
            services.AddTransient<UpdateProofWorkflow>();
            services.AddTransient<IBlockchainRepository, QBitNinjaRepository>();
            services.AddTransient<IBlockchainService, BitcoinService>();

            services.AddTransient<IBlockchainServiceFactory, BlockchainServiceFactory>();

            services.AddTransient<BitcoinService>();
            services.AddTransient<BitcoinTestService>();
            services.AddTransient<ITimestampProofValidator, TimestampProofValidator>();
            services.AddTransient<ITimestampService, TimestampService>();
        }
    }
}
