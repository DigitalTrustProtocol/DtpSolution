using DtpCore.Model;
using MediatR;

namespace DtpCore.Notifications
{
    public class BlockchainProofCreatedNotification : INotification
    {
        public BlockchainProof Proof { get; private set; }

        public BlockchainProofCreatedNotification(BlockchainProof proof)
        {
            Proof = proof;
        }
    }
}
