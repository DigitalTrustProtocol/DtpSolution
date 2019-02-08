using DtpPackageCore.Notifications;
using DtpGraphCore.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpGraphCore.Notifications
{
    public class ClaimReplacedNotificationHandler : INotificationHandler<ClaimReplacedNotification>
    {
        private IGraphClaimService _graphTrustService;
        private ILogger<ClaimAddedNotificationHandler> _logger;

        public ClaimReplacedNotificationHandler(IGraphClaimService graphTrustService, ILogger<ClaimAddedNotificationHandler> logger)
        {
            _graphTrustService = graphTrustService;
            _logger = logger;
        }

        public Task Handle(ClaimReplacedNotification notification, CancellationToken cancellationToken)
        {
            _graphTrustService.Remove(notification.Claim);

            return Unit.Task; // Dummy completed task
        }
    }
}
