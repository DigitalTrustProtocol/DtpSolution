using DtpCore.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using DtpStampCore.Interfaces;
using System.Collections.Concurrent;
using System.Linq;

namespace DtpStampCore.Services
{
    public class TimestampService : ITimestampService
    {

        static ConcurrentDictionary<byte[], BlockchainProof> proofCache = new ConcurrentDictionary<byte[], BlockchainProof>(ByteComparer.EqualityComparer);

        public TrustDBContext DB { get; }
        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IMerkleTree _merkleTree;

        public TimestampService(TrustDBContext dB, IBlockchainServiceFactory blockchainServiceFactory, IMerkleTree merkleTree)
        {
            DB = dB;
            _blockchainServiceFactory = blockchainServiceFactory;
            _merkleTree = merkleTree;
        }

        public BlockchainProof GetProof(Timestamp timestamp)
        {
            var merkleRoot = _merkleTree.ComputeRoot(timestamp.Source, timestamp.Path);

            if (proofCache.TryGetValue(merkleRoot, out BlockchainProof proof))
                return proof;

            if (timestamp.ProofDatabaseID > 0)
            {
                proof = DB.Proofs.FirstOrDefault(p => p.DatabaseID == timestamp.ProofDatabaseID);
                proofCache[merkleRoot] = proof;
                return proof;
            }

            proof = new BlockchainProof();
            var blockchainService = _blockchainServiceFactory.GetService(timestamp.Blockchain);

            proof.Blockchain = timestamp.Blockchain;
            // TODO: The merkletee has to be selective from timestamp object and not predefined by the system.
            proof.MerkleRoot = merkleRoot;

            var addressTimestamp = blockchainService.GetTimestamp(proof.MerkleRoot);

            var derivationStrategy = blockchainService.DerivationStrategy;
            var merkleRootKey = derivationStrategy.GetKey(proof.MerkleRoot);
            proof.Address = derivationStrategy.GetAddress(merkleRootKey);

            proof.Confirmations = addressTimestamp.Confirmations;
            proof.BlockTime = addressTimestamp.Time;

            proofCache[merkleRoot] = proof;

            return proof;
            //if("secp256k1-double256.merkle.dtp1".ToLower().Equals(timestamp.Algorithm))
            //{
            //}
            //return null;
        }
    }
}
