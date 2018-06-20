using Newtonsoft.Json;
using TrustchainCore.Model;

namespace TruststampCore.Model
{
    public class TimestampProof : ProofEntity
    {
        [JsonProperty(PropertyName = "blockchain", NullValueHandling = NullValueHandling.Ignore)]
        public BlockchainProof Blockchain { get; set; }

    }
}
