using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Model
{
    public class Alias
    {
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        public uint Date { get; set; }
    }
}
