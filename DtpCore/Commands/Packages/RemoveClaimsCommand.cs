using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpCore.Commands.Packages
{
    /// <summary>
    /// Claims with issuer id to be removed from DTP Server.
    /// </summary>
    public class RemoveClaimsCommand : IRequest<NotificationSegment>
    {
        public Claim Claim { get; set; }
    }
}
