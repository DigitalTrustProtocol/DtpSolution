using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using DtpCore.Interfaces;

namespace DtpCore.Strategy.Serialization
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public class DIContractResolver : DefaultContractResolver
    {
        IDIMeta _diMeta;
        IServiceProvider _serviceProvider;

        public DIContractResolver(IDIMeta diMeta, IServiceProvider serviceProvider)
        {
            _diMeta = diMeta;
            _serviceProvider = serviceProvider;
        }
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            if (_diMeta.IsRegistred(objectType))
            {
                JsonObjectContract contract = DIResolveContract(objectType);
                contract.DefaultCreator = () => _serviceProvider.GetRequiredService(objectType);

                return contract;
            }

            return base.CreateObjectContract(objectType);
        }

        private JsonObjectContract DIResolveContract(Type objectType)
        {
            var fType = _diMeta.RegistredTypeFor(objectType);
            if (fType != null)
                return base.CreateObjectContract(fType);
            else
                return CreateObjectContract(objectType);
        }
    }
}
