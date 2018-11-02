using DtpCore.Model;
using MediatR;

namespace DtpCore.Commands
{
    public class CurrentBlockchainProofQuery : IRequest<BlockchainProof>
    {
    }
}
