using System;
using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;
using DtpCore.Extensions;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class MerkleTreeSorted : IMerkleTree
    {
        private IList<MerkleNode> LeafNodes { get; }
        public IHashAlgorithm HashAlgorithm { get; }

        public MerkleTreeSorted(IHashAlgorithm hashAlgorithm)
        {
            HashAlgorithm = hashAlgorithm;
            LeafNodes = new List<MerkleNode>();
        }

        public MerkleNode Add(byte[] data)
        {
            return Add(new Timestamp { Source = data });
        }

        public MerkleNode Add(ITimestamp proof)
        {
            var node = new MerkleNode(proof, HashAlgorithm);
            LeafNodes.Add(node);
            return node;
        }

        //public MerkleNode Build()
        //{
        //    var leafs = new List<MerkleNode>();
        //    foreach (var hash in hashs)
        //    {
        //        leafs.Add(new MerkleNode(hash));
        //    }
        //    return Build(leafs);
        //}

        public MerkleNode Build()
        {
            var rootNode = BuildTree(LeafNodes);
            ComputeMerkleTree(rootNode);

            return rootNode;
        }

        private MerkleNode BuildTree(IEnumerable<MerkleNode> leafNodes)
        {
            var nodes = new Queue<MerkleNode>(leafNodes);
            while (nodes.Count > 1)
            {
                var parents = new Queue<MerkleNode>();
                while (nodes.Count > 0)
                {
                    var first = nodes.Dequeue();
                    if(nodes.Count == 0)
                    {
                        parents.Enqueue(first); // Move the odd node up to the next level
                        break;
                    }
                    var second = nodes.Dequeue();

                    if (first.Hash.Compare(second.Hash) < 0)
                    {
                        var hash = HashAlgorithm.HashOf(first.Hash.Combine(second.Hash));
                        parents.Enqueue(new MerkleNode(hash, first, second));
                    }
                    else
                    {
                        var hash = HashAlgorithm.HashOf(second.Hash.Combine(first.Hash));
                        parents.Enqueue(new MerkleNode(hash, second, first));
                    }
                }
                nodes = parents;
            }
            return nodes.FirstOrDefault(); // root
        }

        private void ComputeMerkleTree(MerkleNode root)
        {
            var merkle = new Stack<byte[]>();
            ComputeMerkleTree(root, merkle);
        }

        private void ComputeMerkleTree(MerkleNode node, Stack<byte[]> merkle)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                var tree = new List<byte>();
                foreach (var v in merkle)
                    tree.AddRange(v);

                node.Proof.Receipt = tree.ToArray();
            }

            if (node.Left != null)
            {
                merkle.Push(node.Right.Hash);
                ComputeMerkleTree(node.Left, merkle);
            }

            if (node.Right != null)
            {
                merkle.Push(node.Left.Hash);
                ComputeMerkleTree(node.Right, merkle);
            }

            if (merkle.Count > 0)
                merkle.Pop();

            return;
        }

        public byte[] ComputeRoot(byte[] hash, byte[] path)
        {
            var hashLength = HashAlgorithm.Length;
            if (path != null && path.Length > 0)
            {
                for (var i = 0; i < path.Length; i += hashLength)
                {
                    var merkle = new byte[hashLength];
                    Array.Copy(path, i, merkle, 0, hashLength);
                    if (hash.Compare(merkle) < 0)
                        hash = HashAlgorithm.HashOf(hash.Combine(merkle));
                    else
                        hash = HashAlgorithm.HashOf(merkle.Combine(hash));
                }
            }
            return hash;
        }
    }
}
