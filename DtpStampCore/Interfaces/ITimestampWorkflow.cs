using Newtonsoft.Json;
using DtpCore.Interfaces;
using DtpCore.Model;
using static DtpStampCore.Workflows.TimestampWorkflow;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampWorkflow: IWorkflowContext
    {
        [JsonProperty(PropertyName = "proof", NullValueHandling = NullValueHandling.Ignore)]
        BlockchainProof Proof { get; set; }

        TimestampStates CurrentState { get; set; }
    }
}