using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    /// <summary>
    /// Notify that a Remove Claims message has been issued.
    /// </summary>
    public class ClaimsRemovedNotification : INotification
    {
        public Claim Claim { get; set; }

        public override string ToString()
        {
            return $"Claims with issuer {Claim?.Issuer.Id.ToString()} is removed.";
        }
    }
}
