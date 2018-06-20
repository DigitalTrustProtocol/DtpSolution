using System.Collections.Generic;
using DtpCore.Interfaces;
using DtpStampCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface IBlockchainService
    {
        IBlockchainRepository Repository { get; set; }
        IDerivationStrategy DerivationStrategy { get; set; }
        int VerifyFunds(byte[] key, IList<byte[]> previousTx = null);
        AddressTimestamp GetTimestamp(byte[] merkleRoot);
        IList<byte[]> Send(byte[] hash, byte[] key, IList<byte[]> previousTx = null);
    }
}
