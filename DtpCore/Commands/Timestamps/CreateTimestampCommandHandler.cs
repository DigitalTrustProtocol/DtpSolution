using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Factories;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands
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
                BlockchainProof_db_ID = proof.DatabaseID,
                Algorithm = MerkleStrategyFactory.DOUBLE256_MERKLE_DTP1,
                Blockchain = _configuration.Blockchain(),
                Source = request.Source,
                Registered = DateTime.Now.ToUnixTime()
            };
            return Task.FromResult(timestamp);
        }
    }
}
