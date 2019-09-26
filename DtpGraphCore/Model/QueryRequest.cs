using Newtonsoft.Json;
using System.Collections.Generic;
using DtpGraphCore.Enumerations;
using DtpCore.Model;

namespace DtpGraphCore.Model
{
    /// <summary>
    /// Defines the Query send from the client.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryRequest
    {
        [JsonProperty(PropertyName = "issuer")]
        public Identity Issuer { get; set; }

        [JsonProperty(PropertyName = "subjects")]
        public List<string> Subjects { get; set; }

        /// <summary>
        /// The claim types to search on.
        /// </summary>
        [JsonProperty(PropertyName = "types")]
        public List<string> Types { get; set; }

        /// <summary>
        /// Empty Scope is global.
        /// </summary>
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
        public bool ShouldSerializeScope() { return !string.IsNullOrEmpty(Scope); }

        /// <summary>
        /// Limit the search level. Cannot be more than the predefined max level.
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }
        public bool ShouldSerializeLevel() { return Level > 0; }

        /// <summary>
        /// Specifies how the search should be performed and what results should be returned.
        /// LeafsOnly is default.
        /// </summary>
        [JsonProperty(PropertyName = "flags")]
        public QueryFlags Flags { get; set; }
    }

}
