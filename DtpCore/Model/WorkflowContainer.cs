using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DtpCore.Enumerations;
using DtpCore.Extensions;

namespace DtpCore.Model
{
    [Table("Workflow")]
    public class WorkflowContainer : DatabaseEntity
    {
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Active")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty(PropertyName = "tag", NullValueHandling = NullValueHandling.Ignore)]
        public string Tag { get; set; }

        [UIHint("UnixTime")]
        [JsonProperty(PropertyName = "nextExecution")]
        public long NextExecution { get; set; }

        [UIHint("JSON")]
        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        public WorkflowContainer()
        {
            Active = true;
            NextExecution = DateTime.MinValue.ToUnixTime();
        }
    }
}
