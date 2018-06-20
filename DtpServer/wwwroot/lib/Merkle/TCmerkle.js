/**
A rewrite of the https://github.com/miguelmota/merkle-tree to match the server side implementation of Trustchain.
**/
/**
 * Class reprensenting a Merkle Tree
 * @namespace MerkleTree
 */
var MerkleTree = (function () {
    /**
     * @desc Constructs a Merkle Tree.
     * All nodes and leaves are stored as Buffers.
     * Lonely leaf nodes are promoted to the next level up without being hashed again.
     * @param {Buffer[]} leaves - Array of hashed leaves. Each leaf must be a Buffer.
     * @param {Function} hashAlgorithm - Algorithm used for hashing leaves and nodes
     * @param {Object} options - Additional options
     * @param {Boolean} options.isBitcoinTree - If set to `true`, constructs the Merkle
     * Tree using the [Bitcoin Merkle Tree implementation](http://www.righto.com/2014/02/bitcoin-mining-hard-way-algorithms.html). Enable it when you need
     * to replicate Bitcoin constructed Merkle Trees. In Bitcoin Merkle Trees, single nodes are combined with themselves, and each output hash is hashed again.
     * @example
     * const MerkleTree = require('m-tree')
     * const crypto = require('crypto')
     *
     * function sha256(data) {
     *   // returns Buffer
     *   return crypto.createHash('sha256').update(data).digest()
     * }
     *
     * const leaves = ['a', 'b', 'c'].map(x => sha3(x))
     *
     * const tree = new MerkleTree(leaves, sha256)
     */

    function MerkleTree(leaves, hashAlgorithm, buffer, options = {}) {
        this.hashAlgo = hashAlgorithm
        this.leaves = leaves
        this.layers = [leaves]
        this.isBitcoinTree = !!options.isBitcoinTree
        this.Buffer = buffer;

        this.createHashes(this.leaves)
        this.reverse = function (data) {
            return data;
        }

    }    

    MerkleTree.prototype.concat = function(left, right) {

        if(this.Buffer.compare(left, right) > 0) {
            var temp = left;
            left = right;
            right = temp;
        }

        if (this.isBitcoinTree) {
            data = this.Buffer.concat([this.reverse(left), this.reverse(right)])
        } else {
            data = this.Buffer.concat([left, right])
        }

    }

    MerkleTree.prototype.createHashes = function(nodes) {
        if (!nodes || nodes.length === 1) {
            return false
        }

        var layerIndex = this.layers.length

        this.layers.push([])

        for (var i = 0; i < nodes.length - 1; i += 2) {
            var left = nodes[i]
            var right = nodes[i + 1]
            var data = null

            if (this.isBitcoinTree) {
                data = this.concat([this.reverse(left), this.reverse(right)])
            } else {
                data = this.concat([left, right])
            }

            var hash = this.hashAlgo(data)

            // double hash if bitcoin tree
            if (this.isBitcoinTree) {
                hash = this.reverse(this.hashAlgo(hash))
            }

            this.layers[layerIndex].push(hash)
        }

        // is odd number of nodes
        if (nodes.length % 2 === 1) {
            var data = nodes[nodes.length - 1]
            var hash = data

            // is bitcoin tree
            if (this.isBitcoinTree) {
                // Bitcoin method of duplicating the odd ending nodes
                data = this.concat([this.reverse(data), this.reverse(data)])
                hash = this.hashAlgo(data)
                hash = this.reverse(this.hashAlgo(hash))
            }

            this.layers[layerIndex].push(hash)
        }

        this.createHashes(this.layers[layerIndex])
    }

    /**
     * getLeaves
     * @desc Returns array of leaves of Merkle Tree.
     * @return {Buffer[]}
     * @example
     * const leaves = tree.getLeaves()
     */
    MerkleTree.prototype.getLeaves = function() {
        return this.leaves
    }

    /**
     * getLayers
     * @desc Returns array of all layers of Merkle Tree, including leaves and root.
     * @return {Buffer[]}
     * @example
     * const layers = tree.getLayers()
     */
    MerkleTree.prototype.getLayers = function() {
        return this.layers
    }

    /**
     * getRoot
     * @desc Returns the Merkle root hash as a Buffer.
     * @return {Buffer}
     * @example
     * const root = tree.getRoot()
     */
    MerkleTree.prototype.getRoot = function() {
        return this.layers[this.layers.length - 1][0]
    }

    /**
     * getProof
     * @desc Returns the proof for a target leaf.
     * @param {Buffer} leaf - Target leaf
     * @param {Number} [index] - Target leaf index in leaves array.
     * Use if there are leaves containing duplicate data in order to distinguish it.
     * @return {Buffer[]} - Array of Buffer hashes.
     * @example
     * const proof = tree.getProof(leaves[2])
     *
     * @example
     * const leaves = ['a', 'b', 'a'].map(x => sha3(x))
     * const tree = new MerkleTree(leaves, sha3)
     * const proof = tree.getProof(leaves[2], 2)
     */
    MerkleTree.prototype.getProof = function(leaf, index) {
        var proof = [];

        if (typeof index !== 'number') {
            index = -1

            for (var i = 0; i < this.leaves.length; i++) {
                if (this.Buffer.compare(leaf, this.leaves[i]) === 0) {
                    index = i
                }
            }
        }

        if (index <= -1) {
            return []
        }

        for (var i = 0; i < this.layers.length; i++) {
            var layer = this.layers[i]
            var isRightNode = index % 2
            var pairIndex = (isRightNode ? index - 1 : index + 1)

            if (pairIndex < layer.length) {
                proof.push({
                    position: isRightNode ? 'left' : 'right',
                    data: layer[pairIndex]
                })
            }

            // set index to parent index
            index = (index / 2) | 0
        }

        return proof
    }


    MerkleTree.prototype.calcRoot = function(proof, targetNode) {
        var hash = targetNode

        if (!Array.isArray(proof) ||
            !proof.length) {
            return hash
        }

        var hashLength = 32
        var elements = proof.length / hashLength // Sha256 is 32 bytes

        for (var i = 0; i < elements; i++) {
            var offset = i * hashLength
            
            var buf = new this.Buffer(hashLength)
            var node = address.copy(buf, offset, 0, hashLength)

            var buffers = []

            if (this.isBitcoinTree) {
                buffers.push(this.reverse(hash))

                buffers.push(this.reverse(node.data))

                hash = this.hashAlgo(this.concat(buffers))
                hash = this.reverse(this.hashAlgo(hash))
            } else {
                buffers.push(hash)
                buffers.push(node.data)

                hash = this.hashAlgo(this.concat(buffers))
            }
            
        }
        return hash
    }


    /**
     * verify
     * @desc Returns true if the proof path (array of hashes) can connect the target node
     * to the Merkle root.
     * @param {Buffer[]} proof - Array of proof Buffer hashes that should connect
     * target node to Merkle root.
     * @param {Buffer} targetNode - Target node Buffer
     * @param {Buffer} root - Merkle root Buffer
     * @return {Boolean}
     * @example
     * const root = tree.getRoot()
     * const proof = tree.getProof(leaves[2])
     * const verified = tree.verify(proof, leaves[2], root)
     *
     */
    MerkleTree.prototype.verify = function(proof, targetNode, root) {
        var hash = targetNode

        if (!Array.isArray(proof) ||
            !proof.length ||
            !targetNode ||
            !root) {
            return false
        }

        for (var i = 0; i < proof.length; i++) {
            var node = proof[i]
            var isLeftNode = (node.position === 'left')
            var buffers = []

            if (this.isBitcoinTree) {
                buffers.push(this.reverse(hash))

                buffers.push(this.reverse(node.data))

                hash = this.hashAlgo(this.concat(buffers))
                hash = this.reverse(this.hashAlgo(hash))
            } else {
                buffers.push(hash)
                buffers.push(node.data)

                hash = this.hashAlgo(this.concat(buffers))
            }
        }

        return this.Buffer.compare(hash, root) === 0
    }

    return MerkleTree
}())
