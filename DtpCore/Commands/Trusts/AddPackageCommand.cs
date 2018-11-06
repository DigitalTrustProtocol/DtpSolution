using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpCore.Commands.Trusts
{
    public class AddPackageCommand : IRequest<NotificationSegment>
    {
        public Package Package { get; set; }
    }
}
