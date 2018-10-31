using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Exceptions
{
    public class TrustPackageMissingException : ApplicationException
    {
        public TrustPackageMissingException(string message = "Trust package missing, cannot update") : base(message)
        {
        }
    }
}
