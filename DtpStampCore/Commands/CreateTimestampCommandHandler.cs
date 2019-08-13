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

        public async Task<Timestamp> Handle(CreateTimestampCommand request, CancellationToken cancellationToken)
        {
            var timestamp = await _mediator.Send(new GetTimestampCommand(request.Source, true));
            if (timestamp == null)
            {
                timestamp = new Timestamp
                {
                    Type = Timestamp.DEFAULT_TYPE,
                    Blockchain = _configuration.Blockchain(),
                    Source = request.Source,
                    Registered = DateTime.Now.ToUnixTime()
                };
                _db.Timestamps.Add(timestamp);
            }
            
            return timestamp;
        }
    }
}
