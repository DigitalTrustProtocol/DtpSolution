using DtpCore.Notifications;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class FetchPackageCommand : IRequest<NotificationSegment>
    {
        public PackageMessage PackageMessage { get; }

        public FetchPackageCommand(PackageMessage packageMessage)
        {
            PackageMessage = packageMessage;
        }
    }
}
