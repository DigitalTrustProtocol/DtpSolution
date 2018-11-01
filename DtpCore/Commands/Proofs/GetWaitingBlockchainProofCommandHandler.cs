using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands
{
    public class GetWaitingBlockchainProofCommandHandler : IRequestHandler<GetWaitingBlockchainProofCommand, IEnumerable<BlockchainProof>>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<GetWaitingBlockchainProofCommandHandler> _logger;

        public GetWaitingBlockchainProofCommandHandler(IMediator mediator, TrustDBContext db, ILogger<GetWaitingBlockchainProofCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<IEnumerable<BlockchainProof>> Handle(GetWaitingBlockchainProofCommand request, CancellationToken cancellationToken)
        {
            var proofs = from p in _db.Proofs
                        where (p.Status == ProofStatusType.Waiting.ToString())
                        select p;
            
            return Task.FromResult(proofs.AsEnumerable());
        }


    }
}
