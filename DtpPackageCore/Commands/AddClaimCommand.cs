using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpPackageCore.Commands
{
    public class AddClaimCommand : IRequest<NotificationSegment>
    {
        public Claim Claim { get; set; }
        public Package Package { get; set; }
    }
}
