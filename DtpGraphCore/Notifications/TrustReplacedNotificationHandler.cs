using DtpCore.Notifications;
using DtpGraphCore.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpGraphCore.Notifications
{
    public class TrustReplacedNotificationHandler : INotificationHandler<TrustReplacedNotification>
    {
        private IGraphTrustService _graphTrustService;
        private ILogger<TrustAddedNotificationHandler> _logger;

        public TrustReplacedNotificationHandler(IGraphTrustService graphTrustService, ILogger<TrustAddedNotificationHandler> logger)
        {
            _graphTrustService = graphTrustService;
            _logger = logger;
        }

        public Task Handle(TrustReplacedNotification notification, CancellationToken cancellationToken)
        {
            _graphTrustService.Remove(notification.Trust);

            return Unit.Task; // Dummy completed task
        }
    }
}
