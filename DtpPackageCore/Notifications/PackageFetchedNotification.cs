using DtpCore.Model;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageFetchedNotification : INotification
    {
        public PackageMessage Message { get; }
        public Package Package { get; }

        public PackageFetchedNotification(PackageMessage message, Package package)
        {
            Message = message;
            Package = package;
        }

    }
}
