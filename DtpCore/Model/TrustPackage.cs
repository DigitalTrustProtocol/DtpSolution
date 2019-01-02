using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;


namespace DtpCore.Model
{
    [Table("ClaimPackage")]
    [JsonObject(MemberSerialization.OptIn)]
    public class ClaimPackage
    {
        [JsonProperty(PropertyName = "claimID")]
        public int ClaimID { get; set; }

        [JsonProperty(PropertyName = "claim")]
        public Claim Claim { get; set; }

        [JsonProperty(PropertyName = "packageID")]
        public int PackageID { get; set; }

        [JsonProperty(PropertyName = "package")]
        public Package Package { get; set; }
    }
}
