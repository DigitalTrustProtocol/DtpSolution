using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpCore.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Trusts
{
    public class AddTrustCommandHandler : IRequestHandler<AddTrustCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private ITrustDBService _trustDBService;
        private TrustDBContext _db;
        private NotificationSegment _notifications;
        private readonly ILogger<AddTrustCommandHandler> _logger;

        public AddTrustCommandHandler(IMediator mediator, ITrustDBService trustDBService, TrustDBContext db, NotificationSegment notifications, ILogger<AddTrustCommandHandler> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddTrustCommand request, CancellationToken cancellationToken)
        {
            if (_trustDBService.TrustExist(request.Trust.Id))
            {
                _notifications.Add(new TrustExistNotification { Trust = request.Trust });
                return _notifications;
            }

            var dbTrust = _trustDBService.GetSimilarTrust(request.Trust);
            if (dbTrust != null)
            {
                // TODO: Needs to verfify with Timestamp if exist, for deciding action!
                // The trick is to compare "created" in order to awoid old trust being replayed.
                // For now, we just remove the old trust
                if (dbTrust.Created > request.Trust.Created)
                {
                    //request.Trust.Replaced = true;
                    //_db.Trusts.Add(request.Trust); // Do not add for now

                    _notifications.Add(new TrustObsoleteNotification { OldTrust = request.Trust, ExistingTrust = dbTrust });
                    return _notifications;
                }

                // Check if everything is the same except Created date, then what?
                //trust.Activate 
                //trust.Expire
                //trust.Cost
                //trust.Claim
                //trust.Note

                dbTrust.Replaced = true;
                _trustDBService.Update(dbTrust);

                _notifications.PublishAndWait(new TrustReplacedNotification { Trust = dbTrust });
            }

            request.Trust.Timestamps.Add(_mediator.SendAndWait(new CreateTimestampCommand { Source = request.Trust.Id }));

            // Timestamp objects gets added to the Timestamp table as well!
            _db.Trusts.Add(request.Trust);

            await _notifications.Publish(new TrustAddedNotification { Trust = request.Trust });

            return _notifications;
        }

    }
}
