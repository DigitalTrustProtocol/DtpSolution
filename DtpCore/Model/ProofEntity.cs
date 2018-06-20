using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using TrustchainCore.Interfaces;

namespace TrustchainCore.Model
{
    [Table("Proof")]
    public class ProofEntity : DatabaseEntity,IProof
    {
        [JsonProperty(PropertyName = "source", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Source { get; set; }

        [JsonProperty(PropertyName = "receipt", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Receipt { get; set; }

        [JsonProperty(PropertyName = "registered")]
        public long Registered { get; set; }

        [JsonIgnore]
        public int WorkflowID { get; set; }

        public bool ShouldSerializeRegistered()
        {
            return Registered != 0;
        }
    }
}
