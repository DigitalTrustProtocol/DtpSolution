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
        private readonly IMediator _mediator;
        private readonly IPackageMessageValidator _packageMessageValidator;
        private readonly ITimestampProofValidator _timestampValidator;
        private readonly IPackageService _packageService;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public FetchPackageCommandHandler(IMediator mediator, IPackageMessageValidator packageMessageValidator, ITimestampProofValidator timestampValidator, IPackageService packageService, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _packageMessageValidator = packageMessageValidator;
            _timestampValidator = timestampValidator;
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<Package> Handle(FetchPackageCommand request, CancellationToken cancellationToken)
        {
            _packageMessageValidator.Validate(request.PackageMessage);

            var package = await _packageService.FetchPackageAsync(request.PackageMessage.File);

            await _notifications.Publish(new PackageFetchedNotification(request.PackageMessage, package));

            logger.LogInformation("PackageMessage Received");

            return package;
        }

    }
}
