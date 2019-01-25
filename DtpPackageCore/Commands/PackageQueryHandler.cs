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
        private ITrustDBService _trustDBService;
        private readonly ILogger<PackageQueryHandler> _logger;

        public PackageQueryHandler(IMediator mediator, ITrustDBService trustDBService, ILogger<PackageQueryHandler> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _logger = logger;
        }

        public async Task<IPaginatedList<Package>> Handle(PackageQuery request, CancellationToken cancellationToken)
        {
            var query = _trustDBService.Packages.AsNoTracking();

            //if (request.IncludeClaims)
            //    _trustDBService.LoadPackageClaims()

            if (request.DatabaseID != null)
                query = query.Where(p => p.DatabaseID == request.DatabaseID);

            var list = PaginatedList<Package>.CreateAsync(query, request.PageIndex.GetValueOrDefault(), request.PageSize.GetValueOrDefault());

            return await list;
        }
    }
}
