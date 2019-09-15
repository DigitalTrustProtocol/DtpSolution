using DtpCore.Collections.Generic;
using DtpCore.Extensions;
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

                claim.Subject.Meta.Id = claim.Subject.Id + claim.Scope; // Ensure that the key has an ID value
                var metaEntry = _trustDBContext.Entry(claim.Subject.Meta);
                var dbEntry = _trustDBContext.IdentityMetadata.AsNoTracking().Where(p => p.Id == claim.Subject.Meta.Id).FirstOrDefault();
                if(dbEntry == null)
                {
                    metaEntry.State = EntityState.Added;
                }
                else
                {
                    metaEntry.State = EntityState.Unchanged; // No need to update database to begin with

                    if (metaEntry.Entity.Title != dbEntry.Title)
                        metaEntry.State = EntityState.Modified;

                    if(metaEntry.Entity.Href != dbEntry.Href)
                        metaEntry.State = EntityState.Modified;

                    if (metaEntry.Entity.Href != dbEntry.Href)
                        metaEntry.State = EntityState.Modified;

                    if (metaEntry.Entity.Data.Compare(dbEntry.Data) != 0)
                        metaEntry.State = EntityState.Modified;
                }
                // The entry will be updated in database by a SaveChanges() some where else.
            });
        }
    }
}
