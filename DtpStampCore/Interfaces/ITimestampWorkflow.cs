using Newtonsoft.Json;
using DtpCore.Interfaces;
using DtpCore.Model;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampWorkflow: IWorkflowContext
    {
        [JsonProperty(PropertyName = "proof", NullValueHandling = NullValueHandling.Ignore)]
        BlockchainProof Proof { get; set; }
    }
}