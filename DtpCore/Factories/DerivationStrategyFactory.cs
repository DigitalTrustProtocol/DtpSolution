using Microsoft.Extensions.DependencyInjection;
using DtpCore.Interfaces;
using DtpCore.Strategy;
using System;

namespace DtpCore.Factories
{
    public class DerivationStrategyFactory : IDerivationStrategyFactory
    {

        private IServiceProvider _serviceProvider;

        public DerivationStrategyFactory(IServiceProvider serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public IDerivationStrategy GetService(string name = DerivationSecp256k1PKH.NAME)
        {
            if (String.IsNullOrWhiteSpace(name))
                name = DerivationSecp256k1PKH.NAME;

            Type type = null;
            switch(name.ToLower())
            {
                case DerivationSecp256k1PKH.NAME: type = typeof(DerivationSecp256k1PKH); break;
            }
            if (_serviceProvider == null)
                return (IDerivationStrategy)Activator.CreateInstance(type);

            return (IDerivationStrategy)_serviceProvider.GetRequiredService(type);
        }

    }
}
