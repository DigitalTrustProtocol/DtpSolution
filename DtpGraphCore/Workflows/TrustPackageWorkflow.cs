using DtpGraphCore.Interfaces;
using DtpCore.Services;
using DtpCore.Workflows;
using System;

namespace DtpGraphCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    public class TrustPackageWorkflow : WorkflowContext, ITrustPackageWorkflow
    {
        public TrustPackageWorkflow()
        {
        }

        public override void Execute()
        {
        }
    }

}
