using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageNoClaimsNotification : INotification
    {
        public Package Package { get; }

        public PackageNoClaimsNotification(Package package)
        {
            Package = package;
        }

    }
}
