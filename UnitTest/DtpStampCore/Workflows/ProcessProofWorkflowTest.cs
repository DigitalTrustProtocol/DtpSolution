using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using DtpCore.Workflows;
using UnitTest.DtpStampCore.Mocks;
using QBitNinja.Client.Models;
using System.Collections.Generic;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class ProcessProofWorkflowTest : StartupMock
    {
        [TestMethod]
        public void Serialize()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<UpdateProofWorkflow>();
            var firstTime = workflow.SerializeObject();
            Console.WriteLine(firstTime);
            var wf2 = workflowService.Deserialize<UpdateProofWorkflow>(firstTime);
            var secondTime = wf2.SerializeObject();

            Assert.AreEqual(firstTime, secondTime);
        }


        [TestMethod]
        public void Load()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>(); 
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<UpdateProofWorkflow>();
            Assert.IsNotNull(workflow);
            var id = workflowService.Save(workflow);

            var container = trustDBService.Workflows.FirstOrDefault(p => p.DatabaseID == id);
            Assert.IsNotNull(container);
            Console.WriteLine(container.Data);

            var workflow2 = workflowService.Create(container);
            Assert.IsNotNull(workflow2);
        }

    }
}
