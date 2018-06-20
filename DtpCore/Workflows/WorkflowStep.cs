using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TrustchainCore.Extensions;
using TrustchainCore.Interfaces;

namespace TrustchainCore.Workflows
{
    public abstract class WorkflowStep : IWorkflowStep
    {
        [JsonIgnore]
        public virtual IWorkflowContext Context { get; set; }

        public virtual void Execute()
        {
        }

        public virtual void Dispose()
        {
        }

        public void CombineLog(ILogger logger, string msg)
        {
            logger.DateInformation(Context.ID, msg);
            Context.Log(msg);
        }

    }
}
