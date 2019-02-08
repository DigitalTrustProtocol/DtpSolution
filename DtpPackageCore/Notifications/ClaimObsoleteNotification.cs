using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class ClaimObsoleteNotification : INotification
    {
        public Claim OldClaim { get; set; }
        public Claim ExistingClaim { get; set; }

        public override string ToString()
        {
            return $"Trust {OldClaim?.Id.ToHex()} is obsolete";
        }

    }
}
