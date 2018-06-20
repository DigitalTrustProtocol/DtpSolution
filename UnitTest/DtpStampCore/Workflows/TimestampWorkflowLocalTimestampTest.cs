using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Extensions;
using DtpStampCore.Interfaces;
using DtpStampCore.Repository;
using DtpStampCore.Services;
using DtpStampCore.Workflows;
using UnitTest.DtpStampCore.Mocks;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class TimestampWorkflowLocalTimestampTest : StartupMock
    {
        [TestMethod]
        public void ManyProofs()
        {
            // Setup
            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();

            timestampService.Add(Guid.NewGuid().ToByteArray());
            timestampService.Add(Guid.NewGuid().ToByteArray());
            timestampService.Add(Guid.NewGuid().ToByteArray());

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();


            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.Merkle);

            // No received
            BlockchainRepositoryMock.ReceivedData = @"{
                ""data"" : {
                    }
                }";

            // Test
            workflow.Execute();

            // Verify
            Assert.AreNotEqual(workflow.OutTx.Count, 0);

            Assert.AreEqual(TimestampWorkflow.TimestampStates.AddressVerify, workflow.CurrentState);
        }

        [TestMethod]
        public void AlreadyTimestamp()
        {
            // Setup
            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();

            timestampService.Add(Guid.NewGuid().ToByteArray());
            timestampService.Add(Guid.NewGuid().ToByteArray());
            timestampService.Add(Guid.NewGuid().ToByteArray());

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();

            workflow.SetCurrentState(TimestampWorkflow.TimestampStates.Merkle);

            // No received
            BlockchainRepositoryMock.ReceivedData = @"{
                ""data"" : {
                    ""txs"" : [
                            {
                                ""confirmations"" : 10
                            }
                        ]
                    }
                }";


            // Test
            workflow.Execute();

            // Verify
            Assert.IsNull(workflow.OutTx);

            Assert.AreEqual(TimestampWorkflow.TimestampStates.AddressVerify, workflow.CurrentState);
        }

        //[TestMethod]
        public void RealTimestamp()
        {

            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            IBlockchainRepository _blockchain = new SoChainTransactionRepository(configuration);
            IDerivationStrategyFactory _derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();

            var bitcoinService = new BitcoinService(_blockchain, _derivationStrategyFactory);
            var fundingKeyWIF = configuration.FundingKey();
            var fundingKey = bitcoinService.DerivationStrategy.KeyFromString(fundingKeyWIF);

            var root = Guid.NewGuid().ToByteArray();
            Console.WriteLine("Raw root: "+root.ToHex());

            Key merkleRootKey = new Key(bitcoinService.DerivationStrategy.GetKey(root));
            Console.WriteLine("Root Address: "+merkleRootKey.PubKey.GetAddress(Network.TestNet));
            var serverKey = new Key(fundingKey);
            var serverAddress = serverKey.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine("Funding Address: " + serverAddress);

            var txs = bitcoinService.Send(root, fundingKey);

            foreach (var item in txs)
            {
                var tx = new Transaction(item);
                Console.WriteLine("Transaction:" + tx.ToString());
            }
        }

    }
}
