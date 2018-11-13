using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;


namespace DtpCore.Model
{
    [Table("TrustPackage")]
    [JsonObject(MemberSerialization.OptIn)]
    public class TrustPackage
    {
        [JsonProperty(PropertyName = "trustID")]
        public int TrustID { get; set; }

        [JsonProperty(PropertyName = "trust")]
        public Trust Trust { get; set; }

        [JsonProperty(PropertyName = "packageID")]
        public int PackageID { get; set; }

        [JsonProperty(PropertyName = "package")]
        public Package Package { get; set; }
    }
}
