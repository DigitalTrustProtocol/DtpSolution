using System.Threading;
using System.Threading.Tasks;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using DtpPackageCore.Commands;
using DtpCore.Notifications;
using DtpCore.Interfaces;

namespace DtpServer.Notifications
{
    public class BlockchainProofUpdatedNotificationHandler : INotificationHandler<BlockchainProofUpdatedNotification>
    {
        private IMediator _mediator;
        private ILogger<BlockchainProofUpdatedNotificationHandler> _logger;
        private IPublicFileRepository _publicFileRepository;
        private TrustDBContext _db;

        public BlockchainProofUpdatedNotificationHandler(IMediator mediator, ILogger<BlockchainProofUpdatedNotificationHandler> logger, IPublicFileRepository publicFileRepository, TrustDBContext db)
        {
            _mediator = mediator;
            _logger = logger;
            _publicFileRepository = publicFileRepository;
            _db = db;
        }

        public async Task Handle(BlockchainProofUpdatedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Proof == null)
                return;

            var timestamp = notification.Proof.Timestamps.FirstOrDefault();
            if(timestamp == null)
            {
                timestamp = _db.Timestamps.FirstOrDefault(p => p.BlockchainProof_db_ID == notification.Proof.DatabaseID);
            }

            if (timestamp == null)
                return;

            if(timestamp.PackageDatabase_db_ID > 0)
            {
                var package = (await _mediator.Send(new TrustPackageQuery(timestamp.PackageDatabase_db_ID))).FirstOrDefault();
                
                var name = TrustPackageCreatedNotificationHandler.GetPackageName(package);
                if (!_publicFileRepository.Exist(name))
                {
                    _logger.LogInformation($"Package {name} do not exist on Public File Repository.");
                    return;
                }

                await _publicFileRepository.WriteFileAsync(name, package.ToString());

                _logger.LogInformation($"Package {name} has been updated with Timestamp confirmation.");
            }

            return;
        }
    }
}
