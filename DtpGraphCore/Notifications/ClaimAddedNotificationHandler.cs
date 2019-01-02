using DtpCore.Extensions;
using DtpCore.Notifications;
using DtpGraphCore.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtpGraphCore.Notifications
{
    public class ClaimAddedNotificationHandler : INotificationHandler<ClaimAddedNotification>
    {
        private IGraphClaimService _graphTrustService;
        private ILogger<ClaimAddedNotificationHandler> _logger;

        public ClaimAddedNotificationHandler(IGraphClaimService graphTrustService, ILogger<ClaimAddedNotificationHandler> logger)
        {
            _graphTrustService = graphTrustService;
            _logger = logger;
        }

        public Task Handle(ClaimAddedNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                var trust = notification.Claim;
                var time = DateTime.Now.ToUnixTime();
                if ((trust.Expire == 0 || trust.Expire > time)
                    && (trust.Activate == 0 || trust.Activate <= time))
                    _graphTrustService.Add(trust);    // Add to Graph
            });
        }
    }
}
