using DtpCore.Model;
using MediatR;

namespace DtpCore.Notifications
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
