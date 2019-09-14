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
                
                if (!"alias".EqualsIgnoreCase(claim.Type))
                    return;

                if (claim.Issuer.Id != claim.Subject.Id)
                    return; // Not the same issuer and subject


                var entry = _trustDBContext.IdentityMetadata.Find(claim.Issuer.Id);
                if (entry == null)
                {
                    entry = new IdentityMetadata
                    {
                        Id = claim.Issuer.Id,
                        Data = JsonConvert.SerializeObject(new { alias = claim.Value }, Formatting.None)
                    };
                    _trustDBContext.Add(entry);
                } else
                {
                    var data = JObject.Parse(entry.Data);
                    data["alias"] = claim.Value;
                    entry.Data = data.ToString(Formatting.None);
                }
                // The entry will be updated in database by a SaveChanges() some where else.
            });
        }
    }
}
