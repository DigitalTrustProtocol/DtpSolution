using DtpCore.Interfaces;
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
        IRequestHandler<FetchPackageCommand, NotificationSegment>
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

        public async Task<NotificationSegment> Handle(FetchPackageCommand request, CancellationToken cancellationToken)
        {
            _packageMessageValidator.Validate(request.PackageMessage);

            var package = await _packageService.FetchPackageAsync(request.PackageMessage.Path);

            _notifications.Add(new PackageFetchedNotification(request.PackageMessage, package));

            logger.LogInformation("PackageMessage Received");

            return _notifications;
        }

    }
}
