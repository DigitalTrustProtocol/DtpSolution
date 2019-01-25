using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Packages
{
    public class AddClaimCommandHandler : IRequestHandler<AddClaimCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private NotificationSegment _notifications;
        private IClaimBanListService _claimBanListService;
        private readonly ILogger<AddClaimCommandHandler> _logger;

        public AddClaimCommandHandler(IMediator mediator, ITrustDBService trustDBService, NotificationSegment notifications, IClaimBanListService claimBanListService, ILogger<AddClaimCommandHandler> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _notifications = notifications;
            _claimBanListService = claimBanListService;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddClaimCommand request, CancellationToken cancellationToken)
        {
            // Ensure that banned claims are not added.
            if (_claimBanListService.IsBanned(request.Claim))
            {
                _notifications.Add(new ClaimBannedNotification { Claim = request.Claim });
                return _notifications; // Just return without any processing.
            }

            if (_trustDBService.DoClaimExist(request.Claim.Id))
            {
                // The claim already exist in database, do not process further.
                _notifications.Add(new ClaimExistNotification { Claim = request.Claim });
                return _notifications;
            }

            var dbClaim = _trustDBService.GetSimilarClaim(request.Claim);
            if (dbClaim != null)
            {
                // TODO: Needs to verfify with Timestamp if exist, for deciding action!
                // The trick is to compare "created" in order to awoid old trust being replayed.
                // For now, we just ignore the old trust being added. This may be from packages containing old claims.
                if (dbClaim.Created > request.Claim.Created)
                {

                    request.Claim.ClaimPackages.Add(new ClaimPackageRelationship { Package = request.Package });
                    _trustDBService.Update(request.Claim);

                    _notifications.Add(new ClaimObsoleteNotification { OldClaim = request.Claim, ExistingClaim = dbClaim });
                    return _notifications; // Make sure not to process the old claim.
                }

                // Check if everything is the same except Created date, then what?
                //trust.Activate 
                //trust.Expire
                //trust.Cost
                //trust.Claim
                //trust.Note

                dbClaim.State |= ClaimStateType.Replaced; 
                _trustDBService.Update(dbClaim);

                await _notifications.Publish(new ClaimReplacedNotification { Claim = dbClaim });
            }

            if (_claimBanListService.IsBanClaim(request.Claim))
            {
                request.Claim.State |= ClaimStateType.Functional | ClaimStateType.Ban; // Make the claim as a functional and a Ban claim.

                // Add the claim to the ban list, but only if its is newer. 
                // Should however have been stopped up by the database check.
                if (_claimBanListService.Ensure(request.Claim))
                {
                    _notifications.AddRange(await _mediator.Send(new RemoveClaimsCommand { Claim = request.Claim }));
                }
            }

            // Create the relation between the package and trust
            request.Claim.ClaimPackages.Add(new ClaimPackageRelationship { Package = request.Package });

            // The timestamp feature will be handle by the Server on creating a package for the new claims
            // request.Claim.Timestamps.Add(_mediator.SendAndWait(new CreateTimestampCommand { Source = request.Claim.Id }));

            // Timestamp objects gets added to the Timestamp table as well!
            _trustDBService.Add(request.Claim);

            await _notifications.Publish(new ClaimAddedNotification { Claim = request.Claim });

            return _notifications;
        }

    }
}
