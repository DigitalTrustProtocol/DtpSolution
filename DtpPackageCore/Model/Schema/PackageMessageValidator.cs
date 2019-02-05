using DtpPackageCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Model.Schema
{
    public class PackageMessageValidator : IPackageMessageValidator
    {

        public bool Validate(PackageMessage message)
        {
            return true;
        }
    }
}
