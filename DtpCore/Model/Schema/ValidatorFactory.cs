using DtpCore.Builders;
using DtpCore.Extensions;
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

        public IIdentityValidator GetIdentityValidator(Identity identity)
        {

            var algo = identity.Algorithm;
            if(string.IsNullOrEmpty(identity.Algorithm))
            {
                switch(identity.Type.ToLowerInvariant())
                {
                    case "entity": algo = Secp256k1PKHIdentityValidator.NAME; break;
                    case "thing": algo = Hash160IdentityValidator.NAME; break;
                }
            }

            Type type = null;
            switch (algo.ToLower())
            {
                case Secp256k1PKHIdentityValidator.NAME: type = typeof(Secp256k1PKHIdentityValidator); break;
                //case NumericIdentityValidator.NAME: type = typeof(NumericIdentityValidator); break;
                //case AlphaNumericIdentityValidator.NAME: type = typeof(AlphaNumericIdentityValidator); break;
                case DTPAddressIdentityValidator.NAME: type = typeof(DTPAddressIdentityValidator); break;
                //case UriIdentityValidator.NAME: type = typeof(UriIdentityValidator); break;
                //case StringIdentityValidator.NAME: type = typeof(StringIdentityValidator); break;
                case Hash160IdentityValidator.NAME: type = typeof(Hash160IdentityValidator); break;

                default: return null;
            }

            if (_serviceProvider == null)
                return (IIdentityValidator)Activator.CreateInstance(type);

            return (IIdentityValidator)_serviceProvider.GetRequiredService(type);
        }
    }
}
