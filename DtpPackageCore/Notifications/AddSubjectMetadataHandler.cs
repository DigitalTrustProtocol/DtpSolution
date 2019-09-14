using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class AddSubjectMetadataHandler : INotificationHandler<ClaimAddedNotification>
    {
        private ILogger<AddSubjectMetadataHandler> _logger;
        private TrustDBContext _trustDBContext;

        public AddSubjectMetadataHandler(ILogger<AddSubjectMetadataHandler> logger, TrustDBContext trustDBContext)
        {
            _logger = logger;
            _trustDBContext = trustDBContext;
        }

        public Task Handle(ClaimAddedNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                var claim = notification.Claim;

                if (claim.Subject.Type != "thing")
                    return;

                if (claim.Subject.Meta == null)
                    return;

                var metaEntry = _trustDBContext.Entry(claim.Subject.Meta);
                var dbEntry = _trustDBContext.IdentityMetadata.AsNoTracking().Where(p => p.Id == claim.Subject.Id).FirstOrDefault();
                if(dbEntry == null)
                {
                    metaEntry.State = EntityState.Added;
                }
                else
                {
                    metaEntry.State = EntityState.Modified;
                    var o1 = JObject.Parse(dbEntry.Data);
                    var o2 = JObject.Parse(claim.Subject.Meta.Data);
                    o1.Merge(o2, new JsonMergeSettings
                    {
                        // union array values together to avoid duplicates
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    var result = o1.ToString(Formatting.None);
                    if(result == claim.Subject.Meta.Data)
                    {
                        // No need to update database
                        metaEntry.State = EntityState.Unchanged;
                    }
                }
                // The entry will be updated in database by a SaveChanges() some where else.
            });
        }
    }
}
