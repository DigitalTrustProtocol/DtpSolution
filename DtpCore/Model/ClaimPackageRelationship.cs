using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;


namespace DtpCore.Model
{
    [Table("ClaimPackageRelationship")]
    [JsonObject(MemberSerialization.OptIn)]
    public class ClaimPackageRelationship
    {
        [JsonProperty(PropertyName = "claimID")]
        public int? ClaimID { get; set; }

        [JsonProperty(PropertyName = "claim")]
        public Claim Claim { get; set; }

        [JsonProperty(PropertyName = "packageID")]
        public int? PackageID { get; set; }

        [JsonProperty(PropertyName = "package")]
        public Package Package { get; set; }
    }
}
