using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DtpGraphCore.Model
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GraphIssuer
    {
        [JsonProperty(PropertyName = "address", Order = 10)]
        public string Address;

        [JsonProperty(PropertyName = "index", Order = 10)]
        public int Index;

        [JsonProperty(PropertyName = "referenceId", Order = 20)]
        public int DataBaseID;

        [JsonProperty(PropertyName = "subjects", NullValueHandling = NullValueHandling.Ignore, Order = 100)]
        public Dictionary<int, GraphSubject> Subjects = new Dictionary<int, GraphSubject>();

        public override int GetHashCode()
        {
            return Index;
        }
    }
}
