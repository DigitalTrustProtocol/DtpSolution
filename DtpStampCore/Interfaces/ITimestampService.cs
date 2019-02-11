using DtpCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampService
    {
        BlockchainProof GetProof(Timestamp timestamp);
    }
}