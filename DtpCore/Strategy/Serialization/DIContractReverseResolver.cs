using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using DtpCore.Interfaces;

namespace DtpCore.Strategy.Serialization
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public class DIContractReverseResolver : CamelCasePropertyNamesContractResolver, IContractReverseResolver
    {
        IDIReverseMeta _diReverseMeta;
        IServiceProvider _serviceProvider;

        public DIContractReverseResolver(IDIReverseMeta diReverseMeta, IServiceProvider serviceProvider)
        {
            _diReverseMeta = diReverseMeta;
            _serviceProvider = serviceProvider;
        }
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            if (_diReverseMeta.IsRegistred(objectType))
            {
                JsonObjectContract contract = DIResolveContract(objectType);
                contract.DefaultCreator = () => _serviceProvider.GetRequiredService(objectType);

                return contract;
            }

            return base.CreateObjectContract(objectType);
        }

        private JsonObjectContract DIResolveContract(Type objectType)
        {
            var fType = _diReverseMeta.RegistredTypeFor(objectType);
            if (fType != null)
                return base.CreateObjectContract(fType);
            else
                return CreateObjectContract(objectType);
        }
    }
}
