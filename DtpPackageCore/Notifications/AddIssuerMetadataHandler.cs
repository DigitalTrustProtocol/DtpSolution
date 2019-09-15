using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            return Task.Run(() => {
                var claim = notification.Claim;
                
                if (claim.Issuer.Id != claim.Subject.Id)
                    return; // Not the same issuer and subject


                if (!"alias".EqualsIgnoreCase(claim.Type))
                    return;

                var metadataId = claim.Issuer.Id + claim.Scope;
                var entry = _trustDBContext.IdentityMetadata.Find(metadataId);
                if (entry == null)
                {
                    entry = new IdentityMetadata
                    {
                        Id = metadataId,
                        Title = claim.Value
                    };
                    _trustDBContext.Add(entry);
                } else
                {
                    entry.Title = claim.Value;
                }
                // The entry will be updated in database by a SaveChanges() some where else.
            });
        }
    }
}
