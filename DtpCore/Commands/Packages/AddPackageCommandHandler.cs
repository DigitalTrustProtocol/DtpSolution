using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpCore.ViewModel;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Packages
{
    public class AddPackageCommandHandler : IRequestHandler<AddPackageCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        public AddPackageCommandHandler(IMediator mediator, ITrustDBService trustDBService, NotificationSegment notifications, ILogger<AddPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddPackageCommand request, CancellationToken cancellationToken)
        {
            var package = request.Package;
            var claims = package.Claims;

            _trustDBService.EnsurePackageState(package);

            if (package.State.Match(PackageStateType.Signed))
            {
                // Check for existing packages
                if (_trustDBService.GetPackageById(package.Id) != null)
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    return _notifications;
                }

                _trustDBService.Add(package); // Add package to DBContext
            }
            else
            {
                // Replace package with a build package.
                package = _trustDBService.GetBuildPackage();
            }

            foreach (var claim in claims)
            {
                var claimNotifications = await _mediator.Send(new AddClaimCommand { Claim = claim, Package = package });

                _notifications.AddRange(claimNotifications);
            }

            _trustDBService.SaveChanges();

            await _notifications.Publish(new PackageAddedNotification { Package = package });

            return _notifications;
        }

    }
}
