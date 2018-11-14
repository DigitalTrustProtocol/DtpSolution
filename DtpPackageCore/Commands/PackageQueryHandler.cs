using DtpCore.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class PackageQueryHandler : IRequestHandler<PackageQuery, IPaginatedList<Package>>
    {
        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<PackageQueryHandler> _logger;

        public PackageQueryHandler(IMediator mediator, TrustDBContext db, ILogger<PackageQueryHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public async Task<IPaginatedList<Package>> Handle(PackageQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Packages.AsNoTracking();

            if (request.IncludeTrusts)
                query = query.Include(p => p.Trusts);

            if (request.DatabaseID != null)
                query = query.Where(p => p.DatabaseID == request.DatabaseID);

            var list = PaginatedList<Package>.CreateAsync(query, request.PageIndex.GetValueOrDefault(), request.PageSize.GetValueOrDefault());
            return await list;
        }
    }
}
