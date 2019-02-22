using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class PublishPackageCommand : IRequest<PackageMessage>
    {
        public Package Package { get; }

        public PublishPackageCommand(Package package)
        {
            Package = package;
        }
    }
}
