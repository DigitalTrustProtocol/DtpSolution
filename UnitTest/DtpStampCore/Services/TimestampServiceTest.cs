using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Extensions;
using DtpStampCore.Interfaces;

namespace UnitTest.DtpStampCore.Services
{
    [TestClass]
    public class TimestampServiceTest : StartupMock
    {
        [TestMethod]
        public void AddProof()
        {
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            timestampSynchronizationService.CurrentWorkflowID = 2;
            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();

            var source = Guid.NewGuid().ToByteArray();
            var entity = timestampService.Add(source);

            Assert.IsNotNull(entity);
            Assert.IsTrue(entity.DatabaseID > 0);
            Assert.IsTrue(entity.WorkflowID == timestampSynchronizationService.CurrentWorkflowID);
            Assert.IsTrue(entity.Registered > DateTime.MinValue.ToUnixTime());
            Assert.IsTrue(source.Equals(entity.Source));
        }

        [TestMethod]
        public void GetProof()
        {
            var timestampSynchronizationService = ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            timestampSynchronizationService.CurrentWorkflowID = 2;
            var proofService = ServiceProvider.GetRequiredService<ITimestampService>();
            var source = Guid.NewGuid().ToByteArray();
            var addEntity = proofService.Add(source);

            var getEntity = proofService.Get(source);
            Assert.AreEqual(addEntity.DatabaseID, getEntity.DatabaseID);
            Assert.AreEqual(addEntity.WorkflowID, getEntity.WorkflowID);
            Assert.IsTrue(addEntity.Source.Equals(getEntity.Source));
            Assert.AreEqual(addEntity.Registered, getEntity.Registered);
            Assert.IsTrue(getEntity.Registered > DateTime.MinValue.ToUnixTime());
        }

    }
}
