using Newtonsoft.Json;

namespace DtpCore.Model
{
    public class KeyValue
    {
        [JsonIgnore]
        public int ID { get; set; } // Database row key

        [JsonProperty(PropertyName = "key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "value", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Value { get; set; }
    }
}
