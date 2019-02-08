using DtpCore.Interfaces;
using DtpCore.Notifications;
using DtpCore.Services;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class PublishPackageCommandHandler :
        IRequestHandler<PublishPackageCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private readonly IServerIdentityService serverIdentityService;
        private readonly IPackageService _packageService;
        private NotificationSegment _notifications;
        private readonly ILogger<PublishPackageCommandHandler> logger;

        public PublishPackageCommandHandler(IMediator mediator, IServerIdentityService serverIdentityService, IPackageService packageService, NotificationSegment notifications, ILogger<PublishPackageCommandHandler> logger)
        {
            _mediator = mediator;
            this.serverIdentityService = serverIdentityService;
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(PublishPackageCommand request, CancellationToken cancellationToken)
        {
            if (request.Package == null)
                throw new ApplicationException("Package cannot be null.");

            _notifications.AddRange(await _mediator.Send(new StorePackageCommand(request.Package)));
            var notification = _notifications.FindLast<PackageStoredNotification>();

            _packageService.PublishPackageMessageAsync(notification.Message);

            await _notifications.Publish(new PackagePublishedNotification(request.Package, notification.Message));

            return _notifications;
        }
    }
}
