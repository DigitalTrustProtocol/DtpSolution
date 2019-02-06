using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class StorePackageCommand : IRequest<NotificationSegment>
    {
        public Package Package { get; }

        public StorePackageCommand(Package package)
        {
            Package = package;
        }
    }
}
