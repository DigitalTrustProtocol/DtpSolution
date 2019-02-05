using DtpCore.Model;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageMessageReceived : INotification
    {
        public PackageMessage Message { get; }

        public PackageMessageReceived(PackageMessage message)
        {
            Message = message;
        }

    }
}
