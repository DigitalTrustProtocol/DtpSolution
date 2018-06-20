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

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class TimestampWorkflowTest : StartupMock
    {
        [TestMethod]
        public void Serialize()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();

            var firstTime = workflow.SerializeObject();
            Console.WriteLine(firstTime);
            var wf2 = JsonConvert.DeserializeObject<TimestampWorkflow>(firstTime);
            var secondTime = wf2.SerializeObject();

            Assert.AreEqual(firstTime, secondTime);
        }


        [TestMethod]
        public void Load()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>(); 
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampWorkflow>();
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
