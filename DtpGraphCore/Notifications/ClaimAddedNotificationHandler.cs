using DtpCore.Extensions;
using DtpGraphCore.Interfaces;
using DtpPackageCore.Notifications;
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
                var claim = notification.Claim;


                var time = DateTime.Now.ToUnixTime();
                if ((claim.Expire == 0 || claim.Expire > time)
                    && (claim.Activate == 0 || claim.Activate <= time))
                    _graphTrustService.Add(claim);    // Add to Graph
            });
        }
    }
}
