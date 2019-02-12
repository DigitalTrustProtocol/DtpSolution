﻿using DtpCore.Interfaces;
using DtpCore.Notifications;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class StorePackageCommandHandler :
        IRequestHandler<StorePackageCommand, NotificationSegment>
    {
        private readonly IServerIdentityService _serverIdentityService;
        private readonly IPackageService _packageService;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public StorePackageCommandHandler(IServerIdentityService serverIdentityService, IPackageService packageService, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _serverIdentityService = serverIdentityService;
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(StorePackageCommand request, CancellationToken cancellationToken)
        {
            var message = new PackageMessage
            {
                File = await _packageService.StorePackageAsync(request.Package),
                Scope = request.Package.Scopes ?? "twitter.com",
                ServerId = _serverIdentityService.Id
            };
            message.ServerSignature = _serverIdentityService.Sign(message.ToBinary());

            logger.LogInformation($"Package stored on {message.File}");
            _notifications.Add(new PackageStoredNotification(message, request.Package));

            return _notifications;
        }

    }
}
