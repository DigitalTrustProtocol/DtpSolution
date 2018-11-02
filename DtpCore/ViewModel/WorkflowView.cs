using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace DtpCore.ViewModel
{
    public class WorkflowView
    {
        public int DatabaseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string State { get; set; }
        public string Tag { get; set; }

        [UIHint("JSON")]
        public string Data { get; set; }

        [UIHint("UnixTime")]
        public DateTime LastExecution { get; set; }

        [UIHint("UnixTime")]
        public DateTime NextExecution { get; set; }
    }
}
