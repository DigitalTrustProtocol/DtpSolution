using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Notifications
{
    public class BlockchainProofUpdatedNotification : INotification
    {
        public BlockchainProof Proof { get; private set; }

        public BlockchainProofUpdatedNotification(BlockchainProof proof)
        {
            Proof = proof;
        }
    }
}
