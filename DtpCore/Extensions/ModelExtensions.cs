using System;
using System.Collections.Generic;
using System.Text;
using DtpCore.Model;

namespace DtpCore.Extensions
{
    public static class ModelExtensions
    {
        //public static string GetValue(this Scope scope)
        //{
        //    if (scope == null)
        //        return string.Empty;

        //    return scope.Value;
        //}

        public static void SetSignature(this Package package, byte[] signature)
        {
            if (package.Server == null)
                package.Server = new ServerIdentity();

            package.Server.Proof = signature;
        }
        
    }
}
