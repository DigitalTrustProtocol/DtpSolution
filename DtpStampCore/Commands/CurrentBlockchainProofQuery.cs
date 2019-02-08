using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Commands
{
    public class CurrentBlockchainProofQuery : IRequest<BlockchainProof>
    {
    }
}
