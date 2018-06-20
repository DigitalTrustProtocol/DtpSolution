using Microsoft.Extensions.DependencyInjection;
using System;
using DtpStampCore.Interfaces;
using DtpStampCore.Services;

namespace DtpStampCore.Factories
{
    public class BlockchainServiceFactory : IBlockchainServiceFactory
    {
        private IServiceProvider _serviceProvider;

        public BlockchainServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBlockchainService GetService(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ApplicationException("Name cannot be null or empty");

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
