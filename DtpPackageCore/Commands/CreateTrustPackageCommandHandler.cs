using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class CreateTrustPackageCommandHandler : IRequestHandler<CreateTrustPackageCommand, bool>
    {


        public Task<bool> Handle(CreateTrustPackageCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
