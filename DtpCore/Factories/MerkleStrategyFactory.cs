using System;
using DtpCore.Interfaces;
using DtpCore.Strategy;

namespace DtpCore.Factories
{
    public class MerkleStrategyFactory : IMerkleStrategyFactory
    {
        private IHashAlgorithmFactory _hashAlgorithmFactory;

        public const string DOUBLE256_MERKLE_DTP1 = "double256.merkle.dtp1";

        public MerkleStrategyFactory(IHashAlgorithmFactory hashAlgorithmFactory)
        {
            _hashAlgorithmFactory = hashAlgorithmFactory;
        }

        public IMerkleTree GetStrategy(string name = DOUBLE256_MERKLE_DTP1)
        {
            //if(string.IsNullOrWhiteSpace(name))
            //    name = DOUBLE256_MERKLE_DTP1;

            // Always use default
            return new MerkleTreeSorted(_hashAlgorithmFactory.GetAlgorithm(""));

            //var parts = name.ToLower().Split(".");
            //if (parts.Length != 3)
            //    throw new ApplicationException($"name {name} do not have 3 parts.");

            //var hashAlgorithm = _hashAlgorithmFactory.GetAlgorithm(parts[0]);

            //if (parts[1].Equals("merkle") && parts[2].Equals("dtp1"))
            //    return new MerkleTreeSorted(hashAlgorithm);

            //return null;
        }
    }
}
