using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using DtpCore.Commands;
using DtpCore.Model;
using UnitTest.DtpStampCore.Mocks;
using DtpCore.Enumerations;
using DtpCore.Strategy;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class CreateProofWorkflowMerkleTest : StartupMock
    {
        // No proofs has been added 
        [TestMethod]
        public void Empty()
        {

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<CreateProofWorkflow>();

            workflow.Execute();

            Assert.IsNotNull(workflow.CurrentProof);
            Assert.IsNull(workflow.CurrentProof.MerkleRoot);
            Assert.IsTrue(workflow.Container.Active);

        }

        [TestMethod]
        public void One()
        {
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var derivationStrategy = derivationStrategyFactory.GetService(DerivationSecp256k1PKH.NAME);
            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = derivationStrategy.HashOf(one);

            var proof = Mediator.SendAndWait(new AddNewBlockchainProofCommand());

            proof.Timestamps = new List<Timestamp>
            {
                Mediator.SendAndWait(new CreateTimestampCommand { Source = oneHash })
            };

            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof });

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<CreateProofWorkflow>();
            workflow.Execute();

            var waitingProofs = Mediator.SendAndWait(new WaitingBlockchainProofQuery());
            var waitingProof = waitingProofs.FirstOrDefault();
            Assert.IsNotNull(waitingProof, "Missing waiting proof, has not been created or have an invalid status value.");
            Assert.IsNotNull(waitingProof.MerkleRoot);
        }


        [TestMethod]
        public void Many()
        {
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var derivationStrategy = derivationStrategyFactory.GetService(DerivationSecp256k1PKH.NAME);
            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = derivationStrategy.HashOf(one);


            var proof1 = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            proof1.Status = ProofStatusType.Done.ToString();
            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof1 });

            var proof2 = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            proof2.Status = ProofStatusType.Waiting.ToString();
            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof2 });

            var proof = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            proof.Timestamps = new List<Timestamp>
            {
                Mediator.SendAndWait(new CreateTimestampCommand { Source = oneHash })
            };
            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof });

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<CreateProofWorkflow>();
            workflow.Execute();

            var waitingProofs = Mediator.SendAndWait(new WaitingBlockchainProofQuery());
            Assert.IsTrue(waitingProofs.Count() == 2);
        }
    }
}
