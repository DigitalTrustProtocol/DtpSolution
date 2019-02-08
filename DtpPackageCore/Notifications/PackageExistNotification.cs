using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageExistNotification : INotification
    {
        public Package Package { get; set; }

        public override string ToString()
        {
            return $"Package {Package?.Id.ToHex()} already exist";
        }

    }
}
