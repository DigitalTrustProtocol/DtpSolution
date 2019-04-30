using DtpCore.Interfaces;
using DtpCore.Strategy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DtpCore.Model.Schema
{
    public class ValidatorFactory : IValidatorFactory
    {

        private IConfiguration _configuration;
        private IServiceProvider _serviceProvider;

        public ValidatorFactory(IConfiguration configuration, IServiceProvider serviceProvider = null)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public IIdentityValidator GetIdentityValidator(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                name = NumericIdentityValidator.NAME;

            Type type = null;
            switch (name.ToLower())
            {
                case DerivationSecp256k1PKH.NAME: type = typeof(Secp256k1PKHIdentityValidator); break;
                case NumericIdentityValidator.NAME: type = typeof(NumericIdentityValidator); break;
                case DTPAddressIdentityValidator.NAME: type = typeof(DTPAddressIdentityValidator); break;
                default: return null;
            }

            if (_serviceProvider == null)
                return (IIdentityValidator)Activator.CreateInstance(type);

            return (IIdentityValidator)_serviceProvider.GetRequiredService(type);
        }
    }
}
