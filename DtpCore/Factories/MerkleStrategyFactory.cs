using System;
using DtpCore.Interfaces;
using DtpCore.Strategy;

namespace DtpCore.Factories
{
    public class MerkleStrategyFactory : IMerkleStrategyFactory
    {
        private IHashAlgorithmFactory _hashAlgorithmFactory;

        public const string MERKLE_TC1_DOUBLE256 = "merkle.tc1-double256";

        public MerkleStrategyFactory(IHashAlgorithmFactory hashAlgorithmFactory)
        {
            _hashAlgorithmFactory = hashAlgorithmFactory;
        }

        public IMerkleTree GetStrategy(string name = MERKLE_TC1_DOUBLE256)
        {
            if(string.IsNullOrWhiteSpace(name))
                name = MERKLE_TC1_DOUBLE256;

            var parts = name.ToLower().Split("-");
            if (parts.Length != 2)
                throw new ApplicationException($"name {name} has to many parts.");

            var hashAlgorithm = _hashAlgorithmFactory.GetAlgorithm(parts[1]);

            if (parts[0].Equals("merkle.tc1"))
                return new MerkleTreeSorted(hashAlgorithm);

            return null;
        }
    }
}
