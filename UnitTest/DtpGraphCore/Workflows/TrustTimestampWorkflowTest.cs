using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using TrustchainCore.Builders;
using TrustchainCore.Extensions;
using TrustchainCore.Interfaces;
using TrustchainCore.Services;
using TrustgraphCore.Interfaces;
using TrustgraphCore.Workflows;
using TruststampCore.Interfaces;
using TruststampCore.Workflows;
using UnitTest.TrustchainCore.Extensions;

namespace UnitTest.TruststampCore.Workflows
{
    [TestClass]
    public class TrustTimestampWorkflowTest : StartupMock
    {
        [TestMethod]
        public void Serialize()
        {
            var resolver = ServiceProvider.GetService<IContractResolver>();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = resolver,
                TypeNameHandling = TypeNameHandling.Objects
            };

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = new TrustTimestampWorkflow(workflowService, ServiceProvider);
            workflow.Initialize();

            var reverseResolver = ServiceProvider.GetService<IContractReverseResolver>();
            var settings2 = new JsonSerializerSettings
            {
                ContractResolver = reverseResolver,
                TypeNameHandling = TypeNameHandling.Objects
            };
            var data = JsonConvert.SerializeObject(workflow, Formatting.Indented, settings2);
            Console.WriteLine(data);
            var wf2 = (TrustTimestampWorkflow)JsonConvert.DeserializeObject(data, settings);
            //merkleStep = (IMerkleStep)wf2.Steps[0];

            //Assert.AreEqual(workflow.CurrentStepIndex, wf2.CurrentStepIndex);
            //Assert.AreEqual(((IMerkleStep)workflow.Steps[0]).RootHash[0], ((IMerkleStep)wf2.Steps[0]).RootHash[0]);
        }


        [TestMethod]
        public void Create()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TrustTimestampWorkflow>();
            Assert.IsNotNull(workflow);

            var container = workflowService.CreateWorkflowContainer(workflow);
            Assert.IsNotNull(container);
            Console.WriteLine(container.Data);

            var workflow2 = workflowService.Create(container);
            Assert.IsNotNull(workflow2);
            Assert.AreEqual(workflow.Steps.Count, workflow2.Steps.Count);
        }

        [TestMethod]
        public void Load()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>(); 
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TrustTimestampWorkflow>();
            Assert.IsNotNull(workflow);
            var id = workflowService.Save(workflow);

            var container = trustDBService.Workflows.FirstOrDefault(p => p.DatabaseID == id);
            Assert.IsNotNull(container);
            Console.WriteLine(container.Data);

            var workflow2 = workflowService.Create(container);
            Assert.IsNotNull(workflow2);
            Assert.AreEqual(workflow.Steps.Count, workflow2.Steps.Count);
        }

        [TestMethod]
        public void Execute()
        {
            var blockchain = "btc-testnet";

            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var proofService = ServiceProvider.GetRequiredService<ITimestampService>();
            var timestampWorkflow = workflowService.Create<TimestampWorkflow>();

            timestampSynchronizationService.CurrentWorkflowID = workflowService.Save(timestampWorkflow);

            var trustBuilder = new TrustBuilder(ServiceProvider);
            trustBuilder.SetServer("testserver");
            trustBuilder.AddTrust("A", "B", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));
            trustBuilder.AddTrust("B", "C", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));
            trustBuilder.AddTrust("C", "D", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));
            trustBuilder.Build().Sign();

            trustDBService.Add(trustBuilder.Package);

            foreach (var trust in trustBuilder.Package.Trusts)
            {
                proofService.Add(trust.Id);
            }

            var merkleStep = timestampWorkflow.GetStep<IMerkleStep>();
            merkleStep.Execute();

            timestampWorkflow.Proof.Blockchain = blockchain;
            timestampWorkflow.Proof.Receipt = Guid.NewGuid().ToByteArray();
            timestampWorkflow.Proof.Confirmations = 1;

            workflowService.Save(timestampWorkflow);
            trustDBService.DBContext.SaveChanges();


            var trustTimestampWorkflow = workflowService.Create<TrustTimestampWorkflow>();
            var trustTimestampStep = trustTimestampWorkflow.GetStep<ITrustTimestampStep>();

            // Now execute!

            trustTimestampStep.Execute();

            foreach (var trust in trustBuilder.Package.Trusts)
            {
                var dbTrust = trustDBService.GetTrustById(trust.Id);

                Assert.IsNotNull(dbTrust);
                foreach (var timestamp in dbTrust.Timestamps)
                {
                    Assert.AreEqual(timestamp.Algorithm, blockchain);
                    Assert.IsNotNull(timestamp.Receipt);
                    Assert.IsTrue(timestamp.Receipt.Length > 0);
                }
            }
        }

    }
}
