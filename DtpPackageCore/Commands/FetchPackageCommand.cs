using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class FetchPackageCommand : IRequest<Package>
    {
        public PackageMessage PackageMessage { get; }

        public FetchPackageCommand(PackageMessage packageMessage)
        {
            PackageMessage = packageMessage;
        }
    }
}
