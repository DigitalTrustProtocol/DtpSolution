using DtpCore.Interfaces;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using DtpPackageCore.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class StorePackageCommandHandler :
        IRequestHandler<StorePackageCommand, NotificationSegment>
    {
        private readonly IPackageService _packageService;
        private readonly TrustDBContext _db;
        private readonly NotificationSegment _notifications;
        private readonly ILogger<BuildPackageCommandHandler> _logger;

        public StorePackageCommandHandler(IPackageService packageService, TrustDBContext db, NotificationSegment notifications, ILogger<BuildPackageCommandHandler> logger)
        {
            _packageService = packageService;
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<NotificationSegment> Handle(StorePackageCommand request, CancellationToken cancellationToken)
        {
            // Save file to Package service
            var file = await _packageService.StorePackageAsync(request.Package);

            request.Package.File = file;
            if (_db.Packages.Local.Contains(request.Package))
            {
                // Only save the File property change!
                _db.Packages.Attach(request.Package).Property("File").IsModified = true;
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation($"Package {request.Package.Id} stored, file name: {file}");
            _notifications.Add(new PackageStoredNotification(file, request.Package));

            return _notifications;
        }

    }
}
