using DtpCore.Interfaces;
using DtpCore.Notifications;
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
        private readonly IServerIdentityService _serverIdentityService;
        private readonly IPackageService _packageService;
        private NotificationSegment _notifications;
        private readonly ILogger<PublishPackageCommandHandler> logger;

        public PublishPackageCommandHandler(IMediator mediator, IServerIdentityService serverIdentityService, IPackageService packageService, NotificationSegment notifications, ILogger<PublishPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _serverIdentityService = serverIdentityService;
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(PublishPackageCommand request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.Package.File))
            {
                logger.LogWarning($"Cannot publish package {request.Package.Id} without a file");
                return _notifications;
            }

            var message = new PackageMessage
            {
                File = request.Package.File,
                Scope = request.Package.Scopes ?? "twitter.com",
                ServerId = _serverIdentityService.Id
            };
            message.ServerSignature = _serverIdentityService.Sign(message.ToBinary());

            _packageService.PublishPackageMessageAsync(message);

            await _notifications.Publish(new PackagePublishedNotification(request.Package, message));

            return _notifications;
        }
    }
}
