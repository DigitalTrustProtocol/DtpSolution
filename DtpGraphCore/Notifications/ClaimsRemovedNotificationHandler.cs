using DtpCore.Builders;
using DtpGraphCore.Interfaces;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpGraphCore.Notifications
{
    public class ClaimsRemovedNotificationHandler : INotificationHandler<ClaimsRemovedNotification>
    {
        private IGraphClaimService _graphTrustService;
        private ILogger<ClaimsRemovedNotification> _logger;

        public ClaimsRemovedNotificationHandler(IGraphClaimService graphTrustService, ILogger<ClaimsRemovedNotification> logger)
        {
            _graphTrustService = graphTrustService;
            _logger = logger;
        }

        public Task Handle(ClaimsRemovedNotification notification, CancellationToken cancellationToken)
        {
            var claim = notification.Claim;
            // If claim is a clear.claim.dtp1 then 
            if (claim.Type != PackageBuilder.REMOVE_CLAIMS_DTP1)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                _graphTrustService.RemoveByIssuer(claim);    // Remove from Graph
            });
        }
    }
}
