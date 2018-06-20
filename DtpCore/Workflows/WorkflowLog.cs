using System;
using DtpCore.Interfaces;
using DtpCore.Extensions;
using Newtonsoft.Json;

namespace DtpCore.Workflows
{
    public class WorkflowLog : IWorkflowLog
    {
        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; }

        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        public WorkflowLog()
        {
            Time = DateTime.Now.ToUnixTime();
        }
    }
}
