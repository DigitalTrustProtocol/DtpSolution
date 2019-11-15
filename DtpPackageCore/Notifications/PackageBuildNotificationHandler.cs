using DtpCore.Notifications;
using DtpPackageCore.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class PackageBuildNotificationHandler : INotificationHandler<PackageBuildNotification>
    {

        private readonly IMediator _mediator;
        private NotificationSegment _notifications;
        private readonly ILogger<PackageBuildNotificationHandler> _logger;

        public PackageBuildNotificationHandler(IMediator mediator, NotificationSegment notifications, ILogger<PackageBuildNotificationHandler> logger)
        {
            _mediator = mediator;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task Handle(PackageBuildNotification notification, CancellationToken cancellationToken)
        {
            // Store the package at local drive 
            //_notifications.AddRange(await _mediator.Send(new StorePackageCommand(notification.Package)));
        }
    }

}
