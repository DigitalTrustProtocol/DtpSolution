using NBitcoin.Crypto;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class Sha256 : IHashAlgorithm
    {
        public int Length { get; }
        public const string Name = "sha256";
        public string AlgorithmName => Name;

        public Sha256()
        {
            Length = 32; // SHA 256 = 32 bytes
        }

        public byte[] HashOf(byte[] data)
        {
            return Hashes.SHA256(data);
        }


    }
}
