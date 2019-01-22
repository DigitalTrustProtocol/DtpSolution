using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Packages
{
    public class RemoveClaimsCommandHandler : IRequestHandler<RemoveClaimsCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private TrustDBContext _db;
        private NotificationSegment _notifications;
        private readonly ILogger<AddClaimCommandHandler> _logger;

        public RemoveClaimsCommandHandler(IMediator mediator, ITrustDBService trustDBService, TrustDBContext db, NotificationSegment notifications, ILogger<AddClaimCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }


        public async Task<NotificationSegment> Handle(RemoveClaimsCommand request, CancellationToken cancellationToken)
        {
            var claim = request.Claim;

            var claims = from p in _db.Claims
                         where p.Issuer.Id == request.Claim.Issuer.Id
                         && p.Created <= request.Claim.Created
                         && p.Id != request.Claim.Id
                         select p;

            if (!string.IsNullOrEmpty(claim.Scope))
            {
                claims = claims.Where(p => p.Scope == claim.Scope);
            }

            // Load all claims that have same issuer and is older than current and is not the same as current.
            _db.Claims.RemoveRange(claims);


            await _notifications.Publish(new ClaimsRemovedNotification { Claim = request.Claim });
            return _notifications;
        }
    }
}
