﻿using System.Threading;
using System.Threading.Tasks;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DtpServer.Notifications
{
    public class TrustPackageCreatedNotificationHandler : INotificationHandler<TrustPackageBuildNotification>
    {
        private ILogger<TrustPackageCreatedNotificationHandler> _logger;
        private IPublicFileRepository _publicFileRepository;

        public static string GetPackageName(Package package) => $"Package_{package.Id.ToHex()}.json";


        public TrustPackageCreatedNotificationHandler(ILogger<TrustPackageCreatedNotificationHandler> logger, IPublicFileRepository publicFileRepository)
        {
            _logger = logger;
            _publicFileRepository = publicFileRepository;
        }

        public async Task Handle(TrustPackageBuildNotification notification, CancellationToken cancellationToken)
        {
            if (notification.TrustPackage == null || notification.TrustPackage.Id == null)
                return;

            var name = GetPackageName(notification.TrustPackage);
            if (_publicFileRepository.Exist(name))
                return;

            await _publicFileRepository.WriteFileAsync(name, notification.TrustPackage.ToString()).ConfigureAwait(false);

            _logger.LogInformation($"Package {name} has been created on the public file repository.");
        }

    }
}