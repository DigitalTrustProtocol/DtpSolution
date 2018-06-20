using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DtpCore.Services;
using DtpCore.Enumerations;
using DtpCore.Interfaces;
using System.Collections.Generic;
using DtpCore.Workflows;

namespace UnitTest.DtpCore.Workflow
{
    [TestClass]
    public class WorkflowServiceTest : StartupMock
    {

        [TestMethod]
        public void Create()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<WorkflowContext>();
            Assert.IsNotNull(workflow);
            workflow.UpdateContainer();

            var workflow2 = workflowService.Create(workflow.Container);
            Assert.IsNotNull(workflow2);
        }


        [TestMethod]
        public void CreateGeneric()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<WorkflowContext>();
            Assert.IsNotNull(workflow);
            workflow.UpdateContainer();

            var workflow2 = workflowService.Create(workflow.Container);
            Assert.IsNotNull(workflow2);
        }

        [TestMethod]
        public void Save()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<WorkflowContext>();
            Assert.IsNotNull(workflow);

            var id = workflowService.Save(workflow);
            Assert.AreEqual(id, workflow.Container.DatabaseID);
        }


        [TestMethod]
        public void CreateFromDB()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<WorkflowContext>();
            Assert.IsNotNull(workflow);

            var id = workflowService.Save(workflow);
            Assert.AreEqual(id, workflow.Container.DatabaseID);
            var results = workflowService.GetRunningWorkflows();
            var wf = results[0];
            Assert.IsNotNull(wf);


        }

        [TestMethod]
        public void ExecuteOne()
        {
            var executionSynchronizationService = ServiceProvider.GetService<IExecutionSynchronizationService>();

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<WorkflowContext>();
            Assert.IsNotNull(workflow);

            workflow.Execute();
            Assert.AreEqual(WorkflowStatusType.New.ToString(), workflow.Container.State);
        }

        [TestMethod]
        public void ExecuteMany()
        {
            var executionSynchronizationService = ServiceProvider.GetService<IExecutionSynchronizationService>();

            //var list = new List<IWorkflowContext>();
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            for (int i = 0; i < 10; i++)
            {
                var workflow = workflowService.Create<WorkflowContext>();
                workflow.Execute();
                Assert.AreEqual(WorkflowStatusType.New.ToString(), workflow.Container.State);
            }
        }

        [TestMethod]
        public void GetRunningWorkflows()
        {
            // Setup
            var count = 10;
            var list = new List<IWorkflowContext>();
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            for (int i = 0; i < count; i++)
            {
                var workflow = workflowService.Create<WorkflowContext>();
                var id = workflowService.Save(workflow);
                list.Add(workflow);
            }
            var wf = workflowService.Create<WorkflowContext>();
            wf.Container.State = WorkflowStatusType.Finished.ToString();
            workflowService.Save(wf);

            wf = workflowService.Create<WorkflowContext>();
            wf.Container.State = WorkflowStatusType.Failed.ToString();
            workflowService.Save(wf);

            // Execute
            var results = workflowService.GetRunningWorkflows();

            // Verify
            //Assert.AreEqual(count, results.Count);
            foreach (var item in results)
            {
                Assert.IsNotNull(item);
            }
        }


        //[TestMethod]
        //public void RunWorkflowsOne()
        //{
        //    var logger = ServiceProvider.GetRequiredService<ILogger<WorkflowServiceTest>>();

        //    var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
        //    var workflow = workflowService.Create<IWorkflowContext>();

        //    var step = new BlockingWorkflowStep();
        //    workflow.Steps.Add(step);
        //    var id = workflowService.Save(workflow);

        //    logger.LogInformation("Running RunWorkflows");
        //    workflowService.RunWorkflows();
        //    //Task.Delay(5500).Wait();
        //}

        //[TestMethod]
        //public void RunWorkflowsMany()
        //{
        //    var logger = ServiceProvider.GetRequiredService<ILogger<WorkflowServiceTest>>();

        //    var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();

        //    for (int i = 0; i < 5; i++)
        //    {
        //        var workflow = workflowService.Create<IWorkflowContext>();
        //        var step = new BlockingWorkflowStep
        //        {
        //            Seconds = i
        //        };
        //        workflow.Steps.Add(step);
        //        var id = workflowService.Save(workflow);
        //    }
        //    logger.LogInformation("Running RunWorkflows");
        //    workflowService.RunWorkflows();

        //    //Task.Delay(6500).Wait();
        //}


    }
}
