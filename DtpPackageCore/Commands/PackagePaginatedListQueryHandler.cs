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
    public class PackagePaginatedListQueryHandler : IRequestHandler<PackagePaginatedListQuery, IPaginatedList<Package>>
    {
        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private readonly ILogger<PackagePaginatedListQueryHandler> _logger;

        public PackagePaginatedListQueryHandler(IMediator mediator, ITrustDBService trustDBService, ILogger<PackagePaginatedListQueryHandler> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _logger = logger;
        }

        public async Task<IPaginatedList<Package>> Handle(PackagePaginatedListQuery request, CancellationToken cancellationToken)
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
