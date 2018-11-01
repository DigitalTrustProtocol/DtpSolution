using DtpCore.Model;
using MediatR;

namespace DtpCore.Commands
{
    public class GetCurrentBlockchainProofCommand : IRequest<BlockchainProof>
    {
    }
}
