using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using UnitTest.DtpStampCore.Mocks;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class TimestampWorkflowAddressVerifyTest : StartupMock
    {

        

        [TestMethod]
        public void NoConfirmations()
        {
            BlockchainRepositoryMock.ReceivedData = @"{
                ""data"" : {
                    }
                }";

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();
            workflow.Container.NextExecution = 0;
            workflow.Proof = new BlockchainProof
            {
                Blockchain = "btctest",
                MerkleRoot = Guid.NewGuid().ToByteArray(),
                Confirmations = -1
            };

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.AddressVerify);
            workflow.Execute();

            Assert.IsTrue(workflow.Container.NextExecution > 0); // Wait is called
            Assert.AreEqual(-1, workflow.Proof.Confirmations);
        }

        //[TestMethod]
        //public void Unconfirmed()
        //{

        //}

        [TestMethod]
        public void OneConfirmation()
        {
            BlockchainRepositoryMock.ReceivedData = @"{
                ""data"" : {
                    ""txs"" : [
                            {
                                ""confirmations"" : 1
                            }
                        ]
                    }
                }";

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();
            workflow.Container.NextExecution = 0;
            workflow.Proof = new BlockchainProof
            {
                Blockchain = "btctest",
                MerkleRoot = Guid.NewGuid().ToByteArray(),
                Confirmations = -1
            };

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.AddressVerify);
            workflow.Execute();

            Assert.IsTrue(workflow.Container.NextExecution > 0); // Wait is called
            Assert.AreEqual(1, workflow.Proof.Confirmations);
        }

        [TestMethod]
        public void ManyConfirmation()
        {
            BlockchainRepositoryMock.ReceivedData = @"{
                ""data"" : {
                    ""txs"" : [
                            {
                                ""confirmations"" : 10
                            }
                        ]
                    }
                }";

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();
            workflow.Container.NextExecution = 0;
            workflow.Proof = new BlockchainProof
            {
                Blockchain = "btctest",
                MerkleRoot = Guid.NewGuid().ToByteArray(),
                Confirmations = -1
            };

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.AddressVerify);
            workflow.Execute();

            Assert.IsTrue(workflow.Container.NextExecution == 0); // Wait is called
            Assert.AreEqual(10, workflow.Proof.Confirmations);
        }

    }
}
