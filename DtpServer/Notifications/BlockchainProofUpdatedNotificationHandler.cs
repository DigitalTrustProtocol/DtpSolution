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
    public class BlockchainProofUpdatedNotificationHandler : INotificationHandler<BlockchainProofUpdatedNotification>
    {
        private ILogger<BlockchainProofUpdatedNotificationHandler> _logger;
        private PublicFileRepository _publicFileRepository;
        private TrustDBContext _trustDBContext;

        public BlockchainProofUpdatedNotificationHandler(TrustDBContext trustDBContext,  ILogger<BlockchainProofUpdatedNotificationHandler> logger, PublicFileRepository publicFileRepository)
        {
            _trustDBContext = trustDBContext;
            _logger = logger;
            _publicFileRepository = publicFileRepository;
        }

        public Task Handle(BlockchainProofUpdatedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Proof == null)
                return null;


            //if (notification.Stamp.PackageDatabaseID == 0)
            //    return; // Not pointing to a package.

            //var package = _trustDBContext.Packages.FirstOrDefault(p => p.DatabaseID == notification.Stamp.PackageDatabaseID);
            //if (package == null)
            //    return;

            //var name = TrustPackageCreatedNotificationHandler.GetPackageName(package);
            //if (!_publicFileRepository.Exist(name))
            //    return;

            //await _publicFileRepository.WriteFileAsync(name, package.ToString()).ConfigureAwait(false); // Continue with same thread.

            //_logger.LogInformation($"Package {name} has been updated with Timestamp confirmation.");
            return null;
        }
    }
}
