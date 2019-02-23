using DtpCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampService
    {
        byte[] GetMerkleRoot(Timestamp timestamp);
        BlockchainProof GetProof(Timestamp timestamp);
    }
}