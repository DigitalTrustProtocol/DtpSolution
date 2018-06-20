using System;

namespace DtpCore.Interfaces
{
    /// <summary>
    /// http://www.dotnet-programming.com/post/2017/05/08/Aspnet-core-Deserializing-Json-with-Dependency-Injection.aspx
    /// </summary>
    public interface IDIMeta
    {
        bool IsRegistred(Type t);
        Type RegistredTypeFor(Type t);
    }
}
