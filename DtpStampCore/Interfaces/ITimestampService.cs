using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampService
    {
        IQueryable<Timestamp> Timestamps { get; }
        Timestamp Create(byte[] source);
        Timestamp Add(byte[] source, bool save = true);
        Timestamp Get(byte[] source);
        BlockchainProof GetBlockchainTimestamp(byte[] source);
        IList<Timestamp> FillIn(IList<Timestamp> timestamps, byte[] source);
    }
}