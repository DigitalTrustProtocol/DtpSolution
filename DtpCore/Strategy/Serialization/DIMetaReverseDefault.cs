using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using DtpCore.Interfaces;

namespace DtpCore.Strategy.Serialization
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public class DIMetaReverseDefault : IDIReverseMeta
    {
        IDictionary<Type, Type> register = new Dictionary<Type, Type>();
        public DIMetaReverseDefault(IServiceCollection services)
        {
            foreach (var s in services)
            {
                if(s.ImplementationType != null)
                {
                    register[s.ImplementationType] = s.ServiceType;
                }
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
