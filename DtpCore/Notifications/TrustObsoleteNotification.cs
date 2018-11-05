using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpCore.Notifications
{
    public class TrustObsoleteNotification : INotification
    {
        public Trust OldTrust { get; set; }
        public Trust ExistingTrust { get; set; }

        public override string ToString()
        {
            return $"Trust {OldTrust?.Id.ToHex()} is obsolete";
        }

    }
}
