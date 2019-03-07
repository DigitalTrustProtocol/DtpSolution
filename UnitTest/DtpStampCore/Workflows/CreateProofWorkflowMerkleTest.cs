using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Workflows;
using DtpStampCore.Commands;
using DtpCore.Model;
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

            var waitingProofs = Mediator.SendAndWait(new WaitingBlockchainProofQuery());
            Assert.IsTrue(waitingProofs.Count() == 0);

            Assert.IsTrue(workflow.Container.Active);

        }

        [TestMethod]
        public void One()
        {
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var derivationStrategy = derivationStrategyFactory.GetService(DerivationSecp256k1PKH.NAME);
            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = derivationStrategy.HashOf(one);

            Mediator.SendAndWait(new CreateTimestampCommand(oneHash)); // Create do not save to DB!
            DB.SaveChanges();

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

            var proof1 = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            proof1.Status = ProofStatusType.Done.ToString();
            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof1 });

            var proof2 = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            proof2.Status = ProofStatusType.Waiting.ToString();
            Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof2 });

            Mediator.SendAndWait(new CreateTimestampCommand(derivationStrategy.HashOf(Encoding.UTF8.GetBytes("Hello world\n")) ));
            Mediator.SendAndWait(new CreateTimestampCommand(derivationStrategy.HashOf(Encoding.UTF8.GetBytes("Hello world2\n")) ));
            DB.SaveChanges(); 

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<CreateProofWorkflow>();
            workflow.Execute();

            var waitingProofs = Mediator.SendAndWait(new WaitingBlockchainProofQuery());
            Assert.IsTrue(waitingProofs.Count() == 2);
        }
    }
}
