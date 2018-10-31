using System.Threading;
using System.Threading.Tasks;
using DtpCore.Extensions;
using DtpCore.Repository;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace DtpServer.Notifications
{
    public class ConfirmationUpdatedNotificationHandler : INotificationHandler<ConfirmationUpdatedNotification>
    {
        private ILogger<ConfirmationUpdatedNotificationHandler> _logger;
        private PublicFileRepository _publicFileRepository;
        private TrustDBContext _trustDBContext;

        public ConfirmationUpdatedNotificationHandler(TrustDBContext trustDBContext,  ILogger<ConfirmationUpdatedNotificationHandler> logger, PublicFileRepository publicFileRepository)
        {
            _trustDBContext = trustDBContext;
            _logger = logger;
            _publicFileRepository = publicFileRepository;
        }

        public async Task Handle(ConfirmationUpdatedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Stamp == null || notification.Stamp.DatabaseID == 0)
                return;

            if (notification.Stamp.PackageDatabaseID == 0)
                return; // Not pointing to a package.

            var package = _trustDBContext.Packages.FirstOrDefault(p => p.DatabaseID == notification.Stamp.PackageDatabaseID);
            if (package == null)
                return;

            var name = TrustPackageCreatedNotificationHandler.GetPackageName(package);
            if (!_publicFileRepository.Exist(name))
                return;

            await _publicFileRepository.WriteFileAsync(name, package.ToString()).ConfigureAwait(false); // Continue with same thread.

            _logger.LogInformation($"Package {name} has been updated with Timestamp confirmation.");
        }
    }
}
