using MediatR;

namespace DtpPackageCore.Commands
{
    public class CreateTrustPackageCommand : IRequest<bool>
    {
        public string Name { get; set; }
    }
}
