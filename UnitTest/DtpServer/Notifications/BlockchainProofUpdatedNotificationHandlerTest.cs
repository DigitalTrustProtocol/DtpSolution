using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Commands.Trusts;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpCore.Services;
using DtpPackageCore.Workflows;
using DtpServer.Notifications;
using DtpStampCore.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnitTest.DtpCore.Extensions;
using UnitTest.DtpPackage.Mocks;

namespace UnitTest.DtpServer.Notifications
{
    [TestClass]
    public class BlockchainProofUpdatedNotificationHandlerTest : StartupMock
    {

        [TestMethod]
        public void Handler()
        {
            // Create the source
            //var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            //var derivationStrategy = derivationStrategyFactory.GetService("btcpkh");
            //var one = Encoding.UTF8.GetBytes("Hello world\n");
            //var oneHash = derivationStrategy.HashOf(one);

            //// Create the timestamp and proof
            //var proof = Mediator.SendAndWait(new AddNewBlockchainProofCommand());
            //proof.Timestamps = new List<Timestamp>
            //{
            //    Mediator.SendAndWait(new CreateTimestampCommand { Source = oneHash })
            //};

            //Mediator.SendAndWait(new UpdateBlockchainProofCommand { Proof = proof });

            var builder = new TrustBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true);

            var result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust});

            //Assert.IsNull(builder.CurrentTrust.PackageDatabaseID);

            //var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            //trustDBService.Add(builder.CurrentTrust);

            //trustDBService.DBContext.SaveChanges();


            var workflowService = ServiceProvider.GetRequiredService<IWorkflowService>();

            // Create the package
            var createTrustPackageWorkflow = workflowService.Create<CreateTrustPackageWorkflow>();
            createTrustPackageWorkflow.Execute();

            var createProofWorkflow = workflowService.Create<CreateProofWorkflow>();
            createProofWorkflow.Execute();

            var updateProofWorkflow = workflowService.Create<UpdateProofWorkflow>();
            updateProofWorkflow.Execute();

            //var notification = new BlockchainProofUpdatedNotification(proof);
            //var handler = ServiceProvider.GetRequiredService<BlockchainProofUpdatedNotificationHandler>();
            //handler.Handle(notification, default(CancellationToken)).GetAwaiter().GetResult();

            var repository = ServiceProvider.GetRequiredService<IPublicFileRepository>() as PublicFileRepositoryMock;
            Assert.IsNotNull(repository);
            Assert.IsTrue(repository.FileName.Length > 0);
            Assert.IsTrue(repository.FileContent.Length > 0);
            Console.WriteLine(repository.FileContent);
        }
    }
}

