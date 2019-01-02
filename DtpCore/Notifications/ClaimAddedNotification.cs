using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpCore.Notifications
{
    public class ClaimAddedNotification : INotification
    {
        public Claim Claim { get; set; }

        public override string ToString()
        {
            return $"Trust {Claim?.Id.ToHex()} added";
        }
    }
}
