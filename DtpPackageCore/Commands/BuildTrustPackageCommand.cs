using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class BuildTrustPackageCommand : IRequest<Package>
    {
        // Some filtering data, to narrow down the package.
    }
}
