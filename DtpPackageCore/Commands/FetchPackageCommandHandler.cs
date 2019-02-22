using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class FetchPackageCommandHandler :
        IRequestHandler<FetchPackageCommand, Package>
    {
        private readonly IPackageService _packageService;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> _logger;

        public FetchPackageCommandHandler(IPackageService packageService, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _packageService = packageService;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<Package> Handle(FetchPackageCommand request, CancellationToken cancellationToken)
        {
            var package = await _packageService.FetchPackageAsync(request.File);

            await _notifications.Publish(new PackageFetchedNotification(request.File, package));

            _logger.LogInformation("Package loaded from file system");

            return package;
        }

    }
}
