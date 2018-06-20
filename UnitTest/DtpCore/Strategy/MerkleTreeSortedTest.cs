using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Extensions;
using System;
using System.Collections.Generic;
using DtpCore.Factories;
using System.Linq;

namespace UnitTest.DtpCore.Strategy
{
    [TestClass]
    public class MerkleTreeSortedTest : StartupMock
    {

        [TestMethod]
        public void One()
        {
            var merkleFactory = ServiceProvider.GetRequiredService<IMerkleStrategyFactory>();
            var merkle = merkleFactory.GetStrategy(MerkleStrategyFactory.MERKLE_TC1_DOUBLE256);
            var hashAlgorithmFactory = ServiceProvider.GetRequiredService<IHashAlgorithmFactory>();
            var hashAlgorithm = hashAlgorithmFactory.GetAlgorithm(HashAlgorithmFactory.DOUBLE256);

            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = hashAlgorithm.HashOf(one);
            var oneProof = merkle.Add(new Timestamp { Source = one });

            var root = merkle.Build();

            Console.WriteLine($"Root - Hash: {root.Hash.ConvertToHex()}");
            Console.WriteLine($"One  - source: {one.ConvertToHex()} - hash: {oneProof.Hash.ConvertToHex()} -oneHash: {oneHash.ConvertToHex()}");

            Assert.AreEqual(hashAlgorithm.Length, root.Hash.Length, "Root hash has wrong length");
            Assert.IsTrue(oneProof.Hash.Compare(root.Hash) == 0, "Expected and root hash are not the same");
            Assert.AreEqual(0, oneProof.Proof.Receipt.Length);

        }

        [TestMethod]
        public void Two()
        {
            var merkleFactory = ServiceProvider.GetRequiredService<IMerkleStrategyFactory>();
            var merkle = merkleFactory.GetStrategy(MerkleStrategyFactory.MERKLE_TC1_DOUBLE256);
            var hashAlgorithmFactory = ServiceProvider.GetRequiredService<IHashAlgorithmFactory>();
            var hashAlgorithm = hashAlgorithmFactory.GetAlgorithm(HashAlgorithmFactory.DOUBLE256);

            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var two = Encoding.UTF8.GetBytes("Test\n");
            var oneProof = merkle.Add(new Timestamp { Source = one });
            var twoProof = merkle.Add(new Timestamp { Source = two });

            var oneHash = hashAlgorithm.HashOf(one);
            var twoHash = hashAlgorithm.HashOf(two);

            var root = merkle.Build();

            var expectedResult = CombineHash(hashAlgorithm, oneHash, twoHash);

            Console.WriteLine($"Root        - Hash: {root.Hash.ConvertToHex()}");
            Console.WriteLine($"Expected    - Hash: {expectedResult.ConvertToHex()}");

            Console.WriteLine($"One  - source: {one.ConvertToHex()} - hash: {oneProof.Hash.ConvertToHex()} -Receipt: {oneProof.Proof.Receipt.ConvertToHex()}");
            Console.WriteLine($"Two  - source: {two.ConvertToHex()} - hash: {twoProof.Hash.ConvertToHex()} -Receipt: {twoProof.Proof.Receipt.ConvertToHex()}");

            Assert.IsTrue(expectedResult.Compare(root.Hash) == 0, "Expected and root hash are not the same");

            Assert.IsTrue(root.Hash.Compare(CombineHash(hashAlgorithm, oneProof.Hash, oneProof.Proof.Receipt)) == 0, "root and one with receipt are not the same");
            Assert.IsTrue(root.Hash.Compare(CombineHash(hashAlgorithm, twoProof.Hash, twoProof.Proof.Receipt)) == 0, "root and two with receipt are not the same");


        }

        [TestMethod]
        public void Three()
        {
            var merkleFactory = ServiceProvider.GetRequiredService<IMerkleStrategyFactory>();
            var merkle = merkleFactory.GetStrategy(MerkleStrategyFactory.MERKLE_TC1_DOUBLE256);
            var hashAlgorithmFactory = ServiceProvider.GetRequiredService<IHashAlgorithmFactory>();
            var hashAlgorithm = hashAlgorithmFactory.GetAlgorithm(HashAlgorithmFactory.DOUBLE256);

            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var two = Encoding.UTF8.GetBytes("Test\n");
            var three = Encoding.UTF8.GetBytes("Test\n");
            var oneProof = merkle.Add(new Timestamp { Source = one });
            var twoProof = merkle.Add(new Timestamp { Source = two });
            var threeProof = merkle.Add(new Timestamp { Source = three });

            var oneHash = hashAlgorithm.HashOf(one);
            var twoHash = hashAlgorithm.HashOf(two);
            var threeHash = hashAlgorithm.HashOf(three);

            var root = merkle.Build();

            var firstHash = CombineHash(hashAlgorithm, oneHash, twoHash);
            var expectedResult =CombineHash(hashAlgorithm, firstHash, threeHash);
            

            Console.WriteLine($"Root        - Hash: {root.Hash.ConvertToHex()}");
            Console.WriteLine($"Expected    - Hash: {expectedResult.ConvertToHex()}");

            Console.WriteLine($"One  - source: {one.ConvertToHex()} - hash: {oneProof.Hash.ConvertToHex()} -Receipt: {oneProof.Proof.Receipt.ConvertToHex()}");
            Console.WriteLine($"Two  - source: {two.ConvertToHex()} - hash: {twoProof.Hash.ConvertToHex()} -Receipt: {twoProof.Proof.Receipt.ConvertToHex()}");
            Console.WriteLine($"Two  - source: {three.ConvertToHex()} - hash: {threeProof.Hash.ConvertToHex()} -Receipt: {threeProof.Proof.Receipt.ConvertToHex()}");

            Assert.IsTrue(expectedResult.Compare(root.Hash) == 0, "Expected and root hash are not the same");

            var ee = merkle.ComputeRoot(oneProof.Hash, oneProof.Proof.Receipt);
            Assert.IsTrue(root.Hash.Compare(merkle.ComputeRoot(oneProof.Hash, oneProof.Proof.Receipt)) == 0, "root and one with receipt are not the same");
            Assert.IsTrue(root.Hash.Compare(merkle.ComputeRoot(twoProof.Hash, twoProof.Proof.Receipt)) == 0, "root and two with receipt are not the same");
            Assert.IsTrue(root.Hash.Compare(merkle.ComputeRoot(threeProof.Hash, threeProof.Proof.Receipt)) == 0, "root and three with receipt are not the same");
        }

        [TestMethod]
        public void Receipt()
        {
            var merkleFactory = ServiceProvider.GetRequiredService<IMerkleStrategyFactory>();
            var merkle = merkleFactory.GetStrategy(MerkleStrategyFactory.MERKLE_TC1_DOUBLE256);
            var hashAlgorithmFactory = ServiceProvider.GetRequiredService<IHashAlgorithmFactory>();
            var hashAlgorithm = hashAlgorithmFactory.GetAlgorithm(HashAlgorithmFactory.DOUBLE256);

            var length = 101;
            var nodes = new List<MerkleNode>();
            for (int i = 0; i < length; i++)
            {
                var data = Encoding.UTF8.GetBytes($"{i}\n");
                nodes.Add(merkle.Add(new Timestamp { Source = data }));
            }
            var root = merkle.Build();


            var one = nodes[0];
            Console.WriteLine($"Root        - Hash: {root.Hash.ConvertToHex()}");
            Console.WriteLine($"One  - source: {one.Proof.Source.ConvertToHex()} - hash: {one.Hash.ConvertToHex()} -Receipt: {one.Proof.Receipt.ConvertToHex()}");

            var index = 0;
            foreach (var node in nodes)
            {
                var hash = merkle.HashAlgorithm.HashOf(node.Proof.Source);
                Assert.IsTrue(node.Hash.SequenceEqual(hash), "The source and hash are not equal");
                var expectedResult = merkle.ComputeRoot(hash, node.Proof.Receipt);
                Assert.IsTrue(expectedResult.Compare(root.Hash) == 0, $"Expected node number {index} and root hash are not the same");
                index++;
            }

        }

        private byte[] CombineHash(IHashAlgorithm hashAlgorithm, byte[] first, byte[] second)
        {
            return hashAlgorithm.HashOf((first.Compare(second) < 0) ? first.Combine(second) : second.Combine(first));

        }
    }

}