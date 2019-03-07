using DtpCore.Model;
using DtpCore.Repository;
using DtpStampCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class UpdateBlockchainProofCommandHandler : IRequestHandler<UpdateBlockchainProofCommand, BlockchainProof>
    {
        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<UpdateBlockchainProofCommand> _logger;

        public UpdateBlockchainProofCommandHandler(IMediator mediator, TrustDBContext db, ILogger<UpdateBlockchainProofCommand> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public async Task<BlockchainProof> Handle(UpdateBlockchainProofCommand request, CancellationToken cancellationToken)
        {
            _db.Proofs.Update(request.Proof);
            await _db.SaveChangesAsync();

            await _mediator.Publish(new BlockchainProofUpdatedNotification(request.Proof));

            return request.Proof;
        }
    }
}

