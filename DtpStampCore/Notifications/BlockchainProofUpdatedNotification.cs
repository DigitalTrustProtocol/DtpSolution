using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class BlockchainProofUpdatedNotification : INotification
    {
        public BlockchainProof Proof { get; set; }

        public BlockchainProofUpdatedNotification(BlockchainProof proof)
        {
            Proof = proof;
        }
    }
}
