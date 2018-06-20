using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using DtpCore.Interfaces;

namespace DtpCore.Extensions
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public class DIMetaDefault : IDIMeta
    {
        IDictionary<Type, Type> register = new Dictionary<Type, Type>();
        public DIMetaDefault(IServiceCollection services)
        {
            foreach (var s in services)
            {
                register[s.ServiceType] = s.ImplementationType;
            }
        }
        public bool IsRegistred(Type t)
        {
            return register.ContainsKey(t);
        }

        public Type RegistredTypeFor(Type t)
        {
            return register[t];
        }
    }
}
