using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class AddIssuerMetadataHandler : INotificationHandler<ClaimAddedNotification>
    {
        private ILogger<AddSubjectMetadataHandler> _logger;
        private TrustDBContext _trustDBContext;

        public AddIssuerMetadataHandler(ILogger<AddSubjectMetadataHandler> logger, TrustDBContext trustDBContext)
        {
            _logger = logger;
            _trustDBContext = trustDBContext;
        }

        public Task Handle(ClaimAddedNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var claim = notification.Claim;

                if (claim.Issuer.Id != claim.Subject.Id)
                    return; // Not the same issuer and subject


                if (PackageBuilder.ALIAS_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type))
                    UpdateEntry(claim, (entry) => entry.Title = claim.Value);

                if (PackageBuilder.ICON_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type))
                    UpdateEntry(claim, (entry) => entry.Icon = claim.Value);
            });
        }

        private void UpdateEntry(Claim claim, Action<IdentityMetadata> callback)
        {
            var metadataId = claim.Issuer.Id;
            var entry = _trustDBContext.IdentityMetadata.Find(metadataId);

            if (entry == null)
            {
                entry = new IdentityMetadata
                {
                    Id = metadataId,
                };
                callback(entry);

                _trustDBContext.Add(entry);

            }
            else
            {
                callback(entry);
            }
        }
    }
}
