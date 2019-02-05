using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Notifications
{
    public class PackageMessageReceivedHandler : INotificationHandler<PackageMessageReceived>
    {
        private ILogger<PackageMessageReceivedHandler> _logger;

        public PackageMessageReceivedHandler(ILogger<PackageMessageReceivedHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(PackageMessageReceived notification, CancellationToken cancellationToken)
        {
            // Do checks

            // Check DB for similar package.



            return Task.CompletedTask;
        }
    }

}
