using DtpPackageCore.Model;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageMessageValidator
    {
        bool Validate(PackageMessage message);
    }
}