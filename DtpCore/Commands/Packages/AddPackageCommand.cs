using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpCore.Commands.Packages
{
    public class AddPackageCommand : IRequest<NotificationSegment>
    {
        public Package Package { get; set; }
    }
}
