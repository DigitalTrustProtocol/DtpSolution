using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using System;

namespace DtpCore.Factories
{
    public class DerivationStrategyFactory : IDerivationStrategyFactory
    {
        public const string BTC_PKH = "btc-pkh";

        private IServiceProvider _serviceProvider;

        public DerivationStrategyFactory(IServiceProvider serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public IDerivationStrategy GetService(string name = BTC_PKH)
        {
            if (String.IsNullOrWhiteSpace(name))
                name = BTC_PKH;

            Type type = null;
            switch(name.ToLower())
            {
                case "btcpkh": type = typeof(DerivationBTCPKH); break;
                case "btc-pkh": type = typeof(DerivationBTCPKH); break;
            }
            if (_serviceProvider == null)
                return (IDerivationStrategy)Activator.CreateInstance(type);

            return (IDerivationStrategy)_serviceProvider.GetRequiredService(type);
        }

    }
}
