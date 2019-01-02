using DtpCore.Builders;
using DtpCore.Commands.Trusts;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class TrustPackageQueryHandlerTest : StartupMock
    {

        private Claim CreateTrust(string issuer, string subject)
        {
            var builder = new PackageBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust(issuer, subject, true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Trust = trust });
            DB.SaveChanges();
            return trust;
        }


        [TestMethod]
        public void Empty()
        {
            var result = Mediator.SendAndWait(new PackageQuery());
            
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void One()
        {
            CreateTrust("A", "B");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());
            var addedPackage = ((PackageBuildNotification)notifications[0]).TrustPackage;

            var result = Mediator.SendAndWait(new PackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, result[0].Claims.Count);
        }

        [TestMethod]
        public void Two()
        {
            // Create first package
            CreateTrust("A", "B");
            Mediator.SendAndWait(new BuildPackageCommand());

            // Create second package
            CreateTrust("B", "C");
            var notifications = Mediator.SendAndWait(new BuildPackageCommand());
            var addedPackage = ((PackageBuildNotification)notifications[0]).TrustPackage;

            var result = Mediator.SendAndWait(new PackageQuery());
            Assert.AreEqual(2, result.Count);

            result = Mediator.SendAndWait(new PackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, result[0].Claims.Count);
        }
    }
}
