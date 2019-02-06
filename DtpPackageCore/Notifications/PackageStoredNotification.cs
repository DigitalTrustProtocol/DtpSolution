using DtpCore.Model;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageStoredNotification : INotification
    {
        public PackageMessage Message { get; }
        public Package Package { get; }

        public PackageStoredNotification(PackageMessage message, Package package)
        {
            Message = message;
            Package = package;
        }
    }
}
