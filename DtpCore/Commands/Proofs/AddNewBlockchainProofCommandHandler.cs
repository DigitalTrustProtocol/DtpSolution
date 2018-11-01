using DtpCore.Builders;
using DtpCore.Extensions;
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

            return Task.FromResult(proof);
        }
    }
}
