using DtpCore.Extensions;
using DtpCore.Factories;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Strategy;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class CreateTimestampCommandHandler : IRequestHandler<CreateTimestampCommand, Timestamp>
    {
        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateTimestampCommandHandler> _logger;

        public CreateTimestampCommandHandler(IMediator mediator, TrustDBContext db, IConfiguration configuration, ILogger<CreateTimestampCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<Timestamp> Handle(CreateTimestampCommand request, CancellationToken cancellationToken)
        {
            var proof = _mediator.SendAndWait(new CurrentBlockchainProofQuery());

            var timestamp = new Timestamp
            {
                Proof = proof,
                Algorithm = $"{DerivationSecp256k1PKH.DERIVATION_NAME}-{MerkleStrategyFactory.DOUBLE256_MERKLE_DTP1}",
                Blockchain = _configuration.Blockchain(),
                Source = request.Source,
                Registered = DateTime.Now.ToUnixTime()
            };
            return Task.FromResult(timestamp);
        }
    }
}
