using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
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
        private TrustDBContext _db;
        private NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        public AddPackageCommandHandler(IMediator mediator, TrustDBContext db, NotificationSegment notifications, ILogger<AddPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddPackageCommand request, CancellationToken cancellationToken)
        {
            var package = request.Package;
            var claims = package.Claims;

            // Check for existing packages
            var addPackage = (package.Id != null && package.Id.Length > 0);
            if (addPackage)
            {
                if (_db.Packages.Any(f => f.Id == package.Id))
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    return _notifications;
                }

                package.Claims = null; // Do not update the chilren yet. Special cases for them.
                _db.Packages.Add(package); // Get a DatabaseID for the package.
            }

            var packageResult = new PackageAddedResult();

            foreach (var claim in claims)
            {
                var claimNotifications = await _mediator.Send(new AddClaimCommand { Claim = claim });

                // Add a relationship to a package
                if(addPackage)
                {
                    if (claimNotifications.LastOrDefault() is ClaimObsoleteNotification)
                    {
                        // Make sure to update old trust even that its not active any more. The package history of a trust is still relevant.
                        var notification = (ClaimObsoleteNotification)claimNotifications.LastOrDefault();
                        notification.OldClaim.ClaimPackages.Add(new ClaimPackageRelationship { Package = package });
                        _db.Update(notification.OldClaim);
                    }
                    else
                    {
                        // Create the relation between the package and trust
                        claim.ClaimPackages.Add(new ClaimPackageRelationship { Package = package });
                    }
                }

                _notifications.AddRange(claimNotifications);

                packageResult.Claims.Add(new ClaimAddedResult { ID = claim.Id, Message = claimNotifications.LastOrDefault()?.ToString() ?? "Unknown" });
            }

            _db.SaveChanges();

            await _notifications.Publish(new PackageAddedNotification { Package = package, Result = packageResult });

            return _notifications;
        }

    }
}
