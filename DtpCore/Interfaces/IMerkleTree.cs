using System.Collections.Generic;
using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IMerkleTree
    {
        MerkleNode Add(byte[] data);
        MerkleNode Add(ITimestamp proof);
        MerkleNode Build();
        byte[] ComputeRoot(byte[] hash, byte[] path);
        IHashAlgorithm HashAlgorithm { get; }
    }
}