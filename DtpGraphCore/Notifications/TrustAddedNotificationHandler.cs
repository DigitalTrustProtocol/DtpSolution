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
    public class TrustAddedNotificationHandler : INotificationHandler<TrustAddedNotification>
    {
        private IGraphTrustService _graphTrustService;
        private ILogger<TrustAddedNotificationHandler> _logger;

        public TrustAddedNotificationHandler(IGraphTrustService graphTrustService, ILogger<TrustAddedNotificationHandler> logger)
        {
            _graphTrustService = graphTrustService;
            _logger = logger;
        }

        public Task Handle(TrustAddedNotification notification, CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                var trust = notification.Trust;
                var time = DateTime.Now.ToUnixTime();
                if ((trust.Expire == 0 || trust.Expire > time)
                    && (trust.Activate == 0 || trust.Activate <= time))
                    _graphTrustService.Add(trust);    // Add to Graph
            });
        }
    }
}
