using Newtonsoft.Json;
using System.Collections.Generic;
using DtpGraphCore.Enumerations;

namespace DtpGraphCore.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TrustScope
    {
        /// <summary>
        /// Empty Scope is global.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type;
        public bool ShouldSerializeType()
        {
            return !string.IsNullOrEmpty(Type);
        }

        [JsonProperty(PropertyName = "value")]
        public string Value;
        public bool ShouldSerializeValue()
        {
            return !string.IsNullOrEmpty(Value);
        }
    }


    /// <summary>
    /// Defines the Query send from the client.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryRequest
    {
        [JsonProperty(PropertyName = "issuers")]
        public string Issuer;

        [JsonProperty(PropertyName = "subjects")]
        public List<SubjectQuery> Subjects;

        /// <summary>
        /// The claim types to search on.
        /// </summary>
        [JsonProperty(PropertyName = "types")]
        public List<string> Types;

        /// <summary>
        /// Empty Scope is global.
        /// </summary>
        [JsonProperty(PropertyName = "scope")]
        public TrustScope Scope;
        public bool ShouldSerializeClaimScope()
        {
            return Scope != null;
        }

        /// <summary>
        /// Limit the search level. Cannot be more than the predefined max level.
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public int Level;
        public bool ShouldSerializeLevel()
        {
            return Level > 0;
        }

        /// <summary>
        /// Specifies how the search should be performed and what results should be returned.
        /// LeafsOnly is default.
        /// </summary>
        [JsonProperty(PropertyName = "flags")]
        public QueryFlags Flags;
    }
}
