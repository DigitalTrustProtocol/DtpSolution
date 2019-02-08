using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class ClaimBannedNotification : INotification
    {
        public Claim Claim { get; set; }

        public override string ToString()
        {
            return $"Claim {Claim?.Id.ToHex()} is banned";
        }

    }
}
