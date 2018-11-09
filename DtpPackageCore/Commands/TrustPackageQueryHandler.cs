using DtpCore.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class TrustPackageQueryHandler : IRequestHandler<TrustPackageQuery, IPaginatedList<Package>>
    {
        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<TrustPackageQueryHandler> _logger;

        public TrustPackageQueryHandler(IMediator mediator, TrustDBContext db, ILogger<TrustPackageQueryHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public async Task<IPaginatedList<Package>> Handle(TrustPackageQuery request, CancellationToken cancellationToken)
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
