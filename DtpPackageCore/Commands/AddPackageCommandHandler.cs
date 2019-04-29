using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class AddPackageCommandHandler : IRequestHandler<AddPackageCommand, NotificationSegment>
    {

        private readonly IMediator _mediator;
        private readonly TrustDBContext _db;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        private Dictionary<string, Package> _packageCache = new Dictionary<string, Package>();

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

            _db.EnsurePackageState(package);
            Func<string, Package> getPackage = GetPackage; 

            if (package.State.Match(PackageStateType.Signed))
            {
                // Check for existing packages
                if (await _db.DoPackageExistAsync(package.Id))
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    _logger.LogInformation($"Package {package.Id.ToHex()} already exist in database");
                    return _notifications;
                }

                _db.Packages.Add(package); // Add package to DBContext
                getPackage = (scope) => package; // Replace default function and just return the inline package
            }

            foreach (var claim in claims)
            {
                claim.Id = PackageBuilder.GetClaimID(claim); // Make sure that the claim has an ID for the database.

                var claimNotifications = await _mediator.Send(new AddClaimCommand { Claim = claim, Package = getPackage(claim.Scope) });

                _notifications.AddRange(claimNotifications);
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Package Added Database ID: {package.DatabaseID}");

            await _notifications.Publish(new PackageAddedNotification { Package = package });

            return _notifications;
        }

        /// <summary>
        /// Gets the build package for the scope, uses caching.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Package GetPackage(string scope)
        {
            if (_packageCache.TryGetValue(scope, out Package package))
                return package;

            package = _db.GetBuildPackage(scope);
            _packageCache.Add(scope, package);
            return package;
        }

    }
}
