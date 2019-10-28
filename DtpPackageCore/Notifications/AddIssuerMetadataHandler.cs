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


                if (!PackageBuilder.ALIAS_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type) ||
                    !PackageBuilder.ICON_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type))
                    return;

                var metadataId = claim.Issuer.Id;
                var entry = _trustDBContext.IdentityMetadata.Find(metadataId);


                if (PackageBuilder.ALIAS_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type))
                    UpdateEntry(claim, metadataId, entry, (entry) => entry.Title = claim.Value);// The entry will be updated in database by a SaveChanges() some where else.

                if (PackageBuilder.ICON_IDENTITY_DTP1.EqualsIgnoreCase(claim.Type))
                    UpdateEntry(claim, metadataId, entry, (entry) => entry.Icon = claim.Value);// The entry will be updated in database by a SaveChanges() some where else.
            });
        }

        private void UpdateEntry(Claim claim, string metadataId, IdentityMetadata entry, Action<IdentityMetadata> callback)
        {
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
