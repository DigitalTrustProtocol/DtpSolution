using DtpPackageCore.Model;
using System.Collections.Generic;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageMessageValidator
    {
        void Validate(PackageMessage message);
        bool Validate(PackageMessage message, out IList<string> errors);
    }
}