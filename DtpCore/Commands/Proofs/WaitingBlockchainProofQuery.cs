using DtpCore.Model;
using MediatR;
using System.Collections.Generic;

namespace DtpCore.Commands
{
    public class WaitingBlockchainProofQuery : IRequest<IEnumerable<BlockchainProof>>
    {
    }
}
