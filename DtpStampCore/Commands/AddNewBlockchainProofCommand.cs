using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Commands
{
    public class AddNewBlockchainProofCommand : IRequest<BlockchainProof>
    {
    }
}
