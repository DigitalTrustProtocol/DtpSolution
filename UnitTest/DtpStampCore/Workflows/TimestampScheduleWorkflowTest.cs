using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Services;
using DtpStampCore.Workflows;

namespace UnitTest.DtpStampCore.Workflows
{
    [TestClass]
    public class TimestampScheduleWorkflowTest : StartupMock
    {
        [TestMethod]
        public void Serialize()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            var workflow = new TimestampScheduleWorkflow();
            workflow.WorkflowService = workflowService;

            var firstTime = workflow.SerializeObject();
            Console.WriteLine(firstTime);
            var wf2 = JsonConvert.DeserializeObject<TimestampScheduleWorkflow>(firstTime);
            var secondTime = wf2.SerializeObject();

            Assert.AreEqual(firstTime, secondTime);
        }


        [TestMethod]
        public void ExecuteWithNoProof()
        {
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampScheduleWorkflow>();
            workflow.Execute();

            Assert.AreEqual(WorkflowStatusType.Waiting.ToString(), workflow.Container.State);
            Assert.IsTrue(timestampSynchronizationService.CurrentWorkflowID == 0);
            var saveCurrentID = timestampSynchronizationService.CurrentWorkflowID;

            workflow.Execute();
            Assert.AreEqual(saveCurrentID, timestampSynchronizationService.CurrentWorkflowID);
            Assert.AreEqual(WorkflowStatusType.Waiting.ToString(), workflow.Container.State);
        }

        [TestMethod]
        public void ExecuteWithProof()
        {
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TimestampScheduleWorkflow>();
            workflow.Execute();

            Assert.AreEqual(WorkflowStatusType.Waiting.ToString(), workflow.Container.State);
            Assert.IsTrue(timestampSynchronizationService.CurrentWorkflowID == 0);
            var saveCurrentID = timestampSynchronizationService.CurrentWorkflowID;

            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();
            timestampService.Add(Guid.NewGuid().ToByteArray());
            workflow.Container.NextExecution = DateTime.MinValue.ToUnixTime();

            workflow.Execute();
            Assert.AreNotEqual(saveCurrentID, timestampSynchronizationService.CurrentWorkflowID);
            Assert.AreEqual(WorkflowStatusType.Waiting.ToString(), workflow.Container.State);
        }

    }
}
