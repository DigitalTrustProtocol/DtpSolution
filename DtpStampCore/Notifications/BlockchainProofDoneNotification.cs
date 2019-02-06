using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Notifications
{
    public class BlockchainProofDoneNotification : INotification
    {
        public BlockchainProof Proof { get; private set; }

        public BlockchainProofDoneNotification(BlockchainProof proof)
        {
            Proof = proof;
        }
    }
}
