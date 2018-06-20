using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class TimestampWorkflowMerkleTest : StartupMock
    {
        // No proofs has been added 
        [TestMethod]
        public void Empty()
        {

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.Merkle);
            workflow.Execute();

            Assert.IsNotNull(workflow.Proof);
            Assert.IsNull(workflow.Proof.MerkleRoot);

            Assert.IsFalse(workflow.Container.Active);

        }

        [TestMethod]
        public void One()
        {
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var derivationStrategy = derivationStrategyFactory.GetService("btcpkh");
            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();

            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = derivationStrategy.HashOf(one);

            timestampService.Add(one);

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.Merkle);
            workflow.MethodCallback = () => workflow.StopExecution = true;
            workflow.Execute();

            Assert.IsTrue(oneHash.Compare(workflow.Proof.MerkleRoot) == 0, "One hash and root hash are not the same");

            Assert.IsTrue(workflow.CurrentState == TimestampWorkflow.TimestampStates.Timestamp);
            Assert.IsTrue(workflow.Container.Active);
        }


        [TestMethod]
        public void Execute()
        {
            var proofService = ServiceProvider.GetRequiredService<ITimestampService>();

            var proofOne = Guid.NewGuid().ToByteArray();
            proofService.Add(proofOne);
            proofService.Add(Guid.NewGuid().ToByteArray());
            proofService.Add(Guid.NewGuid().ToByteArray());

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.Merkle);
            workflow.MethodCallback = () => workflow.StopExecution = true;
            workflow.Execute();

            Assert.IsTrue(workflow.CurrentState == TimestampWorkflow.TimestampStates.Timestamp);
            Assert.IsTrue(workflow.Container.Active);

            var proofOneEntity = proofService.Get(proofOne);
            Assert.IsTrue(proofOneEntity.Receipt.Length > 0, "Proof one entity Receipt is not added");
        }
    }
}
