using DtpCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface IBlockchainProofFactory
    {
        BlockchainProof Create(Timestamp timestamp);
    }
}