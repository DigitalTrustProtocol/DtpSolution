using DtpCore.Interfaces;
using DtpCore.Notifications;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model.Database;
using DtpPackageCore.Notifications;

namespace DtpPackageCore.Commands
{
    public class RemoveClaimsCommandHandler : IRequestHandler<RemoveClaimsCommand, NotificationSegment>
    {

        //private readonly IMediator _mediator;
        private readonly TrustDBContext _db;
        private readonly NotificationSegment _notifications;
        //private readonly ILogger<AddClaimCommandHandler> _logger;

        public RemoveClaimsCommandHandler(TrustDBContext db, NotificationSegment notifications) // ILogger<AddClaimCommandHandler> logger
        {
            //_mediator = mediator;
            _db = db;
            _notifications = notifications;
            //_logger = logger;
        }


        public async Task<NotificationSegment> Handle(RemoveClaimsCommand request, CancellationToken cancellationToken)
        {
            var claims = from p in _db.Claims.Include(cp => cp.ClaimPackages).ThenInclude(p=>p.Package)
                         where p.Issuer.Id == request.Claim.Issuer.Id
                         && p.Created <= request.Claim.Created
                         && p.Id != request.Claim.Id
                         select p;

            if (!string.IsNullOrEmpty(request.Claim.Scope))
            {
                claims = claims.Where(p => p.Scope == request.Claim.Scope);
            }

            // Mark all packages as obsolete as they contain claims for removal.
            foreach (var claim in claims)
            {
                foreach (var relationship in claim.ClaimPackages)
                {
                    relationship.Package.State |= PackageStateType.Obsolete;
                }
            }

            // Load all claims that have same issuer and is older than current and is not the same as current.
            _db.Claims.RemoveRange(claims);

            await _notifications.Publish(new ClaimsRemovedNotification { Claim = request.Claim });
            return _notifications;
        }
    }
}
