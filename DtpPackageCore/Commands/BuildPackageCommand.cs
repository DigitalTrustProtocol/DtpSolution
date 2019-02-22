using DtpCore.Model;
using DtpCore.Notifications;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class BuildPackageCommand : IRequest<Package>
    {
        public Package SourcePackage { get; set; }

        public BuildPackageCommand(Package sourcePackage)
        {
            SourcePackage = sourcePackage;
        }
    }
}
