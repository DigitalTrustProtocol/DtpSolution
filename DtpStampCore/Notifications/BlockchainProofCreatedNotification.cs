using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Notifications
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
