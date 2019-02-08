using DtpCore.Enumerations;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class WaitingBlockchainProofQueryHandler : IRequestHandler<WaitingBlockchainProofQuery, IEnumerable<BlockchainProof>>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<WaitingBlockchainProofQueryHandler> _logger;

        public WaitingBlockchainProofQueryHandler(IMediator mediator, TrustDBContext db, ILogger<WaitingBlockchainProofQueryHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<IEnumerable<BlockchainProof>> Handle(WaitingBlockchainProofQuery request, CancellationToken cancellationToken)
        {
            var proofs = _db.Proofs
                        .Include(p => p.Timestamps)
                        .Where(p => p.Status == ProofStatusType.Waiting.ToString())
                        .Select(p => p);

            return Task.FromResult(proofs.AsEnumerable());
        }


    }
}
