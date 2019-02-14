using DtpPackageCore.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTest.TestData;

namespace UnitTest.DtpPackage.Workflows
{
    [TestClass]
    public class SynchronizePackageWorkflowTest : StartupMock
    {
        //[TestMethod]
        public void One()
        {
            var package = TestPackage.CreateBinary(1);

            var workflow = WorkflowService.Create<SynchronizePackageWorkflow>();

            workflow.Execute();
        }
    }
}
