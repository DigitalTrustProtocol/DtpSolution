using DtpCore.Model;
using MediatR;
using System.Collections.Generic;

namespace DtpCore.Commands
{
    public class GetWaitingBlockchainProofCommand : IRequest<IEnumerable<BlockchainProof>>
    {
    }
}
