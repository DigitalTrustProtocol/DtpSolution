using TrustStampCore.Repository;
using TrustStampCore.Service;
using TrustStampCore.Extensions;
using System;
using Newtonsoft.Json.Linq;

namespace TruststampCore.Workflows
{
    public class RemotePayWorkflow : BlockchainWorkflowBatch
    {
        public JValue Retry { get; set; }

        public override bool Initialize()
        {
            base.Initialize();
            if (!EnsureDependencies())
                return false;

            var remotepay = CurrentBatch["state"].EnsureObject("remotepay");
            Retry = remotepay.EnsureProperty("retry", 0);
            return true;
        }

        public override void Execute()
        {
            WriteLog("Waiting for payment on Batch root");

            var rootKey = Bitcoin.GetKey(Root);
            var rootAddress = rootKey.PubKey.GetAddress(BlockchainFactory.GetBitcoinNetwork());
            var info = BlockchainRepository.GetAddressInfo(rootAddress.ToWif());


            if(info != null && info["totalreceived"] != null || info["totalreceived"].ToInteger() > 0)
            {
                // payment has been made!
                WriteLog("Payment has been made on Batch root");
                Push(new SuccessWorkflow());
                return;
            }

            // Wait some time to see if someone pays for the Batch root!
            Retry.Value = (int)Retry + 1;
            if ((int)Retry >= 3)
                Push(new FailedWorkflow("Failed 3 times waiting for payment on Root."));
            else
                Push(new SleepWorkflow(DateTime.Now.AddHours(3), Name)); // Sleep and retry this workflow

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
