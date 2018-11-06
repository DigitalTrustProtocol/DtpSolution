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
    public class BuildTrustPackageCommandHandlerTest : StartupMock
    {

        private Trust CreateTrust(string issuer, string subject)
        {
            var builder = new TrustBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust(issuer, subject, true);
            NotificationSegment result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });
            DB.SaveChanges();
            return trust;
        }


        [TestMethod]
        public void Empty()
        {
            var notifications = Mediator.SendAndWait(new BuildTrustPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is TrustPackageNoTrustNotification);
        }

        [TestMethod]
        public void One()
        {
            CreateTrust("A", "B");

            var notifications = Mediator.SendAndWait(new BuildTrustPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is TrustPackageBuildNotification);
            Assert.AreEqual(1, ((TrustPackageBuildNotification)notifications[0]).TrustPackage.Trusts.Count);

            var packageID = DB.Packages.First().DatabaseID; 
            var packageIds = DB.Trusts.Select(p => p.PackageDatabaseID);
            foreach (var id in packageIds)
            {
                Assert.AreEqual(packageID, id);
            }


        }

        [TestMethod]
        public void Two()
        {
            CreateTrust("A", "B");
            CreateTrust("B", "C");

            var notifications = Mediator.SendAndWait(new BuildTrustPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is TrustPackageBuildNotification);
            Assert.AreEqual(2, ((TrustPackageBuildNotification)notifications[0]).TrustPackage.Trusts.Count);

            var packageID = DB.Packages.First().DatabaseID;
            var trusts = DB.Trusts.Select(p => p);
            foreach (var trust in trusts)
            {
                Assert.AreEqual(packageID, trust.PackageDatabaseID);
            }
        }
    }
}
