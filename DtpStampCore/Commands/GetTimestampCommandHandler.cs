using DtpCore.Extensions;
using DtpCore.Factories;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Strategy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class GetTimestampCommandHandler : IRequestHandler<GetTimestampCommand, Timestamp>
    {
        private TrustDBContext _db;

        public GetTimestampCommandHandler(TrustDBContext db)
        {
            _db = db;
        }

        public async Task<Timestamp> Handle(GetTimestampCommand request, CancellationToken cancellationToken)
        {
            var query = (request.IncludeProof)
                ? _db.Timestamps.Include(p => p.Proof)
                : _db.Timestamps.AsQueryable();

            var timestamp = await query.FirstOrDefaultAsync(p => p.Source == request.Source);

            return timestamp;
        }
    }
}
