using DtpCore.Model;
using DtpCore.Notifications;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class PublishPackageCommand : IRequest<NotificationSegment>
    {
        public Package Package { get; }

        public PublishPackageCommand(Package package)
        {
            Package = package;
        }
    }
}
