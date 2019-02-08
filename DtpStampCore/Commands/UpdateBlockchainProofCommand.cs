using DtpCore.Model;
using MediatR;
using System.Collections.Generic;

namespace DtpStampCore.Commands
{
    public class UpdateBlockchainProofCommand : IRequest<BlockchainProof>
    {
        public BlockchainProof Proof { get; set; }
    }
}
