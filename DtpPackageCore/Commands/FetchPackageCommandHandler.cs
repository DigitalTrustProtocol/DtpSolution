using DtpCore.Commands.Packages;
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
        private readonly IPackageService _packageService;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public FetchPackageCommandHandler(IMediator mediator, IPackageMessageValidator packageMessageValidator, IPackageService packageService, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _packageMessageValidator = packageMessageValidator;
            _packageService = packageService;
            _notifications = notifications;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(FetchPackageCommand request, CancellationToken cancellationToken)
        {
            _packageMessageValidator.Validate(request.PackageMessage);

            var package = await _packageService.FetchPackage(request.PackageMessage.Path);

            _notifications.AddRange(await _mediator.Send(new AddPackageCommand { Package = package }));

            _notifications.Add(new PackageFetchedNotification(request.PackageMessage));

            logger.LogInformation("PackageMessage Received and loaded");

            return _notifications;
        }

    }
}
