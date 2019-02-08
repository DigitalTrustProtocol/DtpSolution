using DtpCore.Model;
using DtpCore.Repository;
using DtpStampCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class AddNewBlockchainProofCommandHandler : IRequestHandler<AddNewBlockchainProofCommand, BlockchainProof>
    {
        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<AddNewBlockchainProofCommandHandler> _logger;

        public AddNewBlockchainProofCommandHandler(IMediator mediator, TrustDBContext db, ILogger<AddNewBlockchainProofCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<BlockchainProof> Handle(AddNewBlockchainProofCommand request, CancellationToken cancellationToken)
        {
            var proof = new BlockchainProof();

            _db.Proofs.Add(proof);
            _db.SaveChanges();

            _mediator.Publish(new BlockchainProofCreatedNotification(proof));

            return Task.FromResult(proof);
        }
    }
}
