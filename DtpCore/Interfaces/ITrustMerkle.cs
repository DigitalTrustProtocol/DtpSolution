using Newtonsoft.Json;

namespace DtpCore.Interfaces
{
    public interface ITrustMerkle
    {
        /// <summary>
        /// Default is sha256
        /// </summary>
        [JsonProperty(PropertyName = "hash")]
        string Hash { get; set; }

        /// <summary>
        /// DTP v1 sorted
        /// Default tc1
        /// </summary>
        [JsonProperty(PropertyName = "merkle")]
        string Merkle { get; set; }
    }
}