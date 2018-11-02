using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpPackageCore.Commands;
using DtpPackageCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTest.DtpCore.Extensions;
using UnitTest.DtpPackage.Mocks;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class AddNewTrustPackageCommandHandlerTest : StartupMock
    {

        [TestMethod]
        public void One()
        {
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("testissuer1", "testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();

            var trust = builder.CurrentTrust;
            var timestamp = Mediator.SendAndWait(new CreateTimestampCommand { Source = trust.Id });
            trust.Timestamps = trust.Timestamps ?? new List<Timestamp>();
            trust.Timestamps.Add(timestamp);

            Assert.IsNull(builder.CurrentTrust.PackageDatabaseID);

            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            trustDBService.Add(builder.CurrentTrust);
            trustDBService.DBContext.SaveChanges();

            // Test
            var package = Mediator.Send(new AddNewTrustPackageCommand()).GetAwaiter().GetResult();
            Assert.IsNotNull(package);
            Assert.IsTrue(package.Trusts.Count > 0);
            Assert.AreEqual(package.DatabaseID, package.Trusts[0].PackageDatabaseID);

            Console.WriteLine(package.ToString());
        }
    }
}
