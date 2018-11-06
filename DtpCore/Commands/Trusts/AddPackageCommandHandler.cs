using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpCore.ViewModel;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Trusts
{
    public class AddPackageCommandHandler : IRequestHandler<AddPackageCommand, NotificationSegment>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private NotificationSegment _notifications;
        private readonly ILogger<AddPackageCommandHandler> _logger;

        public AddPackageCommandHandler(IMediator mediator, TrustDBContext db, NotificationSegment notifications, ILogger<AddPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(AddPackageCommand request, CancellationToken cancellationToken)
        {
            var package = request.Package;

            // Check for existing packages
            if ((package.Id != null && package.Id.Length > 0))
            {
                if (_db.Packages.Any(f => f.Id == package.Id))
                {
                    _notifications.Add(new PackageExistNotification { Package = package });
                    return _notifications;
                }
            }

            var packageResult = new PackageAddedResult();

            foreach (var trust in package.Trusts)
            {
                var trustResult = await _mediator.Send(new AddTrustCommand { Trust = trust });

                packageResult.Trusts.Add(new TrustAddedResult { ID = trust.Id, Message = trustResult.LastOrDefault()?.ToString() ?? "Unknown" });

            }

            _db.SaveChanges();

            await _notifications.Publish(new PackageAddedNotification { Package = package, Result = packageResult });

            return _notifications;
        }

    }
}
