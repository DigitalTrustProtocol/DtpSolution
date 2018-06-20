using NBitcoin.Crypto;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class Sha256 : IHashAlgorithm
    {
        public int Length { get; }
        public string AlgorithmName { get; }

        public Sha256()
        {
            Length = 32; // SHA 256 = 32 bytes
            AlgorithmName = "sha256";
        }

        public byte[] HashOf(byte[] data)
        {
            return Hashes.SHA256(data);
        }


    }
}
