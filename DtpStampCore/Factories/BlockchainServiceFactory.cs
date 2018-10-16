using Microsoft.Extensions.DependencyInjection;
using System;
using DtpStampCore.Interfaces;
using DtpStampCore.Services;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;

namespace DtpStampCore.Factories
{
    public class BlockchainServiceFactory : IBlockchainServiceFactory
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        public BlockchainServiceFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public IBlockchainService GetService(string name = null)
        {
            if (String.IsNullOrEmpty(name))
            {
                //var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
                name = _configuration.Blockchain();
                //throw new ApplicationException("Name cannot be null or empty");
            }

            Type type = null;
            switch(name.ToLower())
            {
                case "btc": type = typeof(BitcoinService); break;
                case "btctest": type = typeof(BitcoinTestService); break;
            }

            return (IBlockchainService)_serviceProvider.GetRequiredService(type);
        }
    }
}
