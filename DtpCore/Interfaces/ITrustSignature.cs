using Newtonsoft.Json;

namespace DtpCore.Interfaces
{
    public interface ITrustSignature
    {
        /// <summary>
        /// Standard is the Secp256k1
        /// </summary>
        [JsonProperty(PropertyName = "algorithm")]
        string Algorithm { get; set; }

        /// <summary>
        /// Signature verification
        /// Not included in the Binary payload for signature verification!
        /// </summary>
        [JsonProperty(PropertyName = "signature")]
        byte[] Signature { get; set; }
    }
}