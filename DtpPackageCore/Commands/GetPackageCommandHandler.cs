using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DtpPackageCore.Commands
{ 
    public class GetPackageCommandHandler : IRequestHandler<GetPackageCommand, Package>
    {

        private readonly TrustDBContext DBContext;
        private readonly ILogger<AddClaimCommandHandler> _logger;

        public GetPackageCommandHandler(TrustDBContext dBContext, ILogger<AddClaimCommandHandler> logger)
        {
            DBContext = dBContext;
            _logger = logger;
        }

        public async Task<Package> Handle(GetPackageCommand request, CancellationToken cancellationToken)
        {

            var query = DBContext.Packages.AsQueryable();

            if (request.ID != null)
                query = query.Where(p => p.Id == request.ID);

            if (request.DatabaseID != 0)
                query = query.Where(p => p.DatabaseID == request.DatabaseID);

            if (request.IncludeClaims)
            {
                query = query.Include(p => p.ClaimPackages)
                    .ThenInclude(p => p.Claim);
            }

            query = query.Include(p => p.Timestamps)
                .ThenInclude(p => p.Proof);
            
            var package = await query.FirstOrDefaultAsync();

            if(request.IncludeClaims && package != null)
                package.Claims = package.ClaimPackages.OrderBy(p => p.Claim.DatabaseID).Select(p => p.Claim).ToList();

            return package;
        }
    }
}
