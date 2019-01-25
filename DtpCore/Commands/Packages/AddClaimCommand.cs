using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpCore.Commands.Packages
{
    public class AddClaimCommand : IRequest<NotificationSegment>
    {
        public Claim Claim { get; set; }
        public Package Package { get; set; }
    }
}
