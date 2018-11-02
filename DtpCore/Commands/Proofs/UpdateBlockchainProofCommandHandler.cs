using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Notifications;
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

        public Task<BlockchainProof> Handle(UpdateBlockchainProofCommand request, CancellationToken cancellationToken)
        {
            _db.Proofs.Update(request.Proof);
            _db.SaveChanges();

            _mediator.Publish(new BlockchainProofUpdatedNotification(request.Proof));

            return Task.FromResult(request.Proof);
        }
    }
}

