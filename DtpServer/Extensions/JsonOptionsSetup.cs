using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using Newtonsoft.Json.Serialization;

namespace DtpServer.Extensions
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public class JsonOptionsSetup : IConfigureOptions<MvcNewtonsoftJsonOptions>
    {
        IServiceProvider serviceProvider;
        public JsonOptionsSetup(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void Configure(MvcNewtonsoftJsonOptions o)
        {
            var resover = serviceProvider.GetService<IContractResolver>();
            o.SerializerSettings.ContractResolver = resover;
        }
    }
}
