using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Exceptions
{
    public class TrustPackageExistException : ApplicationException
    {
        public TrustPackageExistException(string message = "Trust package already exist") : base(message)
        {
        }
    }
}
