using DtpCore.Commands.Packages;
using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpStampCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class BlockchainProofDoneNotificationHandler : INotificationHandler<BlockchainProofDoneNotification>
    {

        private readonly IMediator mediator;
        private readonly ILogger<BlockchainProofDoneNotificationHandler> _logger;

        public BlockchainProofDoneNotificationHandler(IMediator mediator, ILogger<BlockchainProofDoneNotificationHandler> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(BlockchainProofDoneNotification notification, CancellationToken cancellationToken)
        {
            foreach (var timestamp in notification.Proof.Timestamps)
            {
                if (timestamp.PackageDatabaseID == null)
                    continue;

                var package = await mediator.Send(new GetPackageCommand { DatabaseID = (int)timestamp.PackageDatabaseID });

                var notifications = await mediator.Send(new PublishPackageCommand(package));
            }
        }
    }

}
