using DtpCore.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DtpCore.Model
{
    [Table("BlockchainProof")]
    [JsonObject(MemberSerialization.OptIn)]
    public class BlockchainProof : DatabaseEntity
    {
        [JsonProperty(PropertyName = "blockchain", NullValueHandling = NullValueHandling.Ignore)]
        public string Blockchain { get; set; }

        [JsonProperty(PropertyName = "merkleRoot", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] MerkleRoot { get; set; }

        [JsonProperty(PropertyName = "receipt", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Receipt { get; set; }

        [JsonProperty(PropertyName = "address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "confirmations")]
        public int Confirmations { get; set; }
        public bool ShouldSerializeConfirmations() => Confirmations != -1;

        [JsonProperty(PropertyName = "blocktime")]
        public long BlockTime { get; set; }
        public bool ShouldSerializeBlockTime() => BlockTime != 0;

        [NotMapped]
        [JsonProperty(PropertyName = "remote", NullValueHandling = NullValueHandling.Ignore)]
        public BlockchainProof Remote { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "retryAttempts")]
        public int RetryAttempts { get; set; }

        [JsonProperty(PropertyName = "timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public List<Timestamp> Timestamps { get; set; }

        public BlockchainProof()
        {
            BlockTime = 0;
            Confirmations = -1;
            Status = ProofStatusType.New.ToString();
            RetryAttempts = 0;
        }

    }
}
