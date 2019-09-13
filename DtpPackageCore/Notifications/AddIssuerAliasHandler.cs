using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class AddIssuerAliasHandler : INotificationHandler<ClaimAddedNotification>
    {
        private ILogger<AddSubjectSourceHandler> _logger;
        private TrustDBContext _trustDBContext;

        public AddIssuerAliasHandler(ILogger<AddSubjectSourceHandler> logger, TrustDBContext trustDBContext)
        {
            _logger = logger;
            _trustDBContext = trustDBContext;
        }

        public Task Handle(ClaimAddedNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                var claim = notification.Claim;
                
                if (string.IsNullOrWhiteSpace(claim.Issuer.Alias))
                    return;

                //var entry = _trustDBContext.KeyValues.Find(claim.Subject.Id);
                //if (entry == null)
                //{
                //    claim.Subject.Source.Id = claim.Subject.Id;
                //    _trustDBContext.SubjectSources.Add(claim.Subject.Source);
                //}
            });
        }
    }
}
