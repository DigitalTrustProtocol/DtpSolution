using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpPackageCore.Commands
{
    public class AddPackageCommand : IRequest<NotificationSegment>
    {
        public Package Package { get; set; }

        public AddPackageCommand(Package package)
        {
            Package = package;
        }
    }
}
