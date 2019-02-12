﻿using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Notifications;
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

        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        private Dictionary<string, Package> _packageCache = new Dictionary<string, Package>();

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
            Func<string, Package> getPackage = GetPackage; 

            if (package.State.Match(PackageStateType.Signed))
            {
                // Check for existing packages
                if (await _trustDBService.DoPackageExistAsync(package.Id))
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    return _notifications;
                }

                // Verify timestamp
                


                _trustDBService.Add(package); // Add package to DBContext
                getPackage = (scope) => package; // Replace default function and just return the inline package
            }

            foreach (var claim in claims)
            {
                var claimNotifications = await _mediator.Send(new AddClaimCommand { Claim = claim, Package = getPackage(claim.Scope) });

                _notifications.AddRange(claimNotifications);
            }

            _trustDBService.SaveChanges();

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

            package = _trustDBService.GetBuildPackage(scope);
            _packageCache.Add(scope, package);
            return package;
        }

    }
}
