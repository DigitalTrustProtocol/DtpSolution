using DtpCore.Interfaces;
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
        //private readonly IServerIdentityService _serverIdentityService;
        private readonly IPackageService _packageService;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public StorePackageCommandHandler(IPackageService packageService, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(StorePackageCommand request, CancellationToken cancellationToken)
        {

            var file = await _packageService.StorePackageAsync(request.Package);

            logger.LogInformation($"Package {request.Package.Id} stored, file name: {file}");
            _notifications.Add(new PackageStoredNotification(file, request.Package));

            return _notifications;
        }

    }
}
