using DtpCore.Model;
using MediatR;
using System.Collections.Generic;

namespace DtpStampCore.Commands
{
    public class WaitingBlockchainProofQuery : IRequest<IEnumerable<BlockchainProof>>
    {
    }
}
