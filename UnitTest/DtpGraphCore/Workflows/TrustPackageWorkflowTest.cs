using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Repository;
using DtpCore.Services;
using DtpGraphCore.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using UnitTest.DtpCore.Repository;

namespace UnitTest.DtpGraphCore.Workflows
{
    [TestClass]
    public class TrustPackageWorkflowTest : StartupMock
    {

        [TestMethod]
        public void Create()
        {
            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TrustPackageWorkflow>();
            Assert.IsNotNull(workflow);
            workflow.UpdateContainer();

            var workflow2 = workflowService.Create(workflow.Container);
            Assert.IsNotNull(workflow2);
        }

        [TestMethod]
        public void Execute()
        {
            // Setup
            var trustDBContext = ServiceProvider.GetRequiredService<TrustDBContext>();
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("A", "B", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true));
            builder.AddTrust("B", "B", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true));
            builder.AddTrust("C", "B", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true));
            builder.Build().Sign();

            trustDBContext.Trusts.AddRange(builder.Package.Trusts);
            trustDBContext.SaveChanges();

            var dbId = builder.Package.Trusts.Last().DatabaseID;

            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();
            var workflow = workflowService.Create<TrustPackageWorkflow>();
            Assert.IsNotNull(workflow);

            // Execution
            workflow.Execute();

            // Test
            Assert.AreEqual(dbId, workflow.LastTrustDatabaseID, "Not the same id!");
            Assert.AreEqual(3, workflow.Builder.Package.Trusts.Count, "Wrong number of trusts");

            var fileRepository = (PublicFileRepositoryMock)workflow.FileRepository;
            Assert.IsNotNull(fileRepository);
            Assert.IsNotNull(fileRepository.FileName);
            Assert.IsNotNull(fileRepository.FileContent);

            Console.WriteLine($"File name: {fileRepository.FileName}");
            Console.WriteLine($"File content:{Environment.NewLine}{fileRepository.FileContent}");
        }
    }
}
