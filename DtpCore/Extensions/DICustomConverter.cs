using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System;

namespace DtpCore.Extensions
{
    public class DICustomConverter<T> : CustomCreationConverter<T> where T : class
    {
        private readonly IServiceProvider _serviceProvider;

        public DICustomConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override T Create(Type objectType)
        {
            return _serviceProvider.GetRequiredService(objectType) as T;
        }

    }
}
