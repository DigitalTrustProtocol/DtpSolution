using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DtpCore.Interfaces;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using DtpCore.Enumerations;
using DtpCore.Services;

namespace UnitTest.DtpStampCore.Services
{
    [TestClass]
    public class TimestampWorkflowServiceTest : StartupMock
    {
        [TestMethod]
        public void EnsureTimestampScheduleWorkflow()
        {
            var timestampWorkflowService = ServiceProvider.GetRequiredService<ITimestampWorkflowService>();
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();

            var noEntity = trustDBService.Workflows.FirstOrDefault(p => p.Type == typeof(CreateProofWorkflow).AssemblyQualifiedName
                                             && p.Active == true);

            Assert.IsNull(noEntity);
            timestampWorkflowService.EnsureTimestampScheduleWorkflow();
            var entity = trustDBService.Workflows.FirstOrDefault(p => p.Type == typeof(CreateProofWorkflow).AssemblyQualifiedName
                                 && p.Active == true);

            Assert.IsNotNull(entity);

            timestampWorkflowService.EnsureTimestampScheduleWorkflow();

            var count = trustDBService.Workflows.Count(p => p.Type == typeof(CreateProofWorkflow).AssemblyQualifiedName
                                 && p.Active == true);

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void EnsureTimestampWorkflow()
        {
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            var timestampWorkflowService = ServiceProvider.GetRequiredService<ITimestampWorkflowService>();
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();

            timestampWorkflowService.CreateTimestampWorkflow();

            var count = trustDBService.Workflows.Count(p => p.Type == typeof(ProcessProofWorkflow).AssemblyQualifiedName
                     && p.Active == true);

            Assert.AreEqual(1, count);
            Assert.IsTrue(timestampSynchronizationService.CurrentWorkflowID > 0);
        }
    }
}
