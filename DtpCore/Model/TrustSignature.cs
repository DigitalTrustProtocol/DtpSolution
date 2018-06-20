using Newtonsoft.Json;
using DtpCore.Interfaces;

namespace DtpCore.Model
{
    public class TrustSignature : ITrustSignature
    {

        /// <summary>
        /// Standard is the Secp256k1
        /// </summary>
        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }


        /// <summary>
        /// Signature verification
        /// Not included in the Binary payload for signature verification!
        /// </summary>
        [JsonProperty(PropertyName = "signature")]
        public byte[] Signature { get; set; }

    }
}
