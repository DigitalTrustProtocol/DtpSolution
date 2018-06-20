using TruststampCore.Repository;
using TruststampCore.Service;
using TrustchainCore.Extensions;
using System.Net;

namespace TruststampCore.Workflows
{
    public class RemoteStampWorkflow : WorkflowBatch
    {


        public override void Execute()
        {
            var remoteEndpoint = App.Config["remoteendpoint"].ToStringValue().Trim();
            if (!VerifyEndpoint(remoteEndpoint)) // No WIF key, then try to stamp remotely
            {
                WriteLog("Invalid remote endpoint"); // No comment!
                Push(new FailedWorkflow());
                return;
            }


            // Check the root at remote!


            // Submit a root!
            var id = ((byte[])CurrentBatch["root"]).ConvertToHex();
            var url = remoteEndpoint + App.Config["remoteport"].ToInteger() + "/api/proof/" + id;
            using (WebClient client = new WebClient())
            {
                var proof = client.UploadString(url, id);
                // Put proof on the CurrentBatch State?
            }
            

            Update();
        }

        private bool VerifyEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                return false;

            return true;
        }

    }
}
