using DtpCore.Builders;
using DtpCore.Commands.Packages;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class BuildTrustPackageCommandHandlerTest : StartupMock
    {

        private Claim CreateTrust(string issuer, string subject)
        {
            var builder = new PackageBuilder();
            var trust = builder.BuildBinaryClaim(issuer, subject, true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust, Package = builder.Package });
            DB.SaveChanges();
            return trust;
        }


        [TestMethod]
        public void Empty()
        {
            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageNoTrustNotification);
        }

        [TestMethod]
        public void One()
        {
            CreateTrust("A", "B");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageBuildNotification);
            Assert.AreEqual(1, ((PackageBuildNotification)notifications[0]).TrustPackage.Claims.Count);

            var packageID = DB.Packages.First().DatabaseID; 
            var packageIds = DB.Claims.Select(p => p.PackageDatabaseID);
            foreach (var id in packageIds)
                Assert.AreEqual(packageID, id);

            foreach (var tp in DB.ClaimPackageRelationships)
            {
                Assert.IsTrue(tp.PackageID == packageID);
                Assert.IsTrue(tp.ClaimID > 0);
            } 
        }

        [TestMethod]
        public void Two()
        {
            CreateTrust("A", "B");
            CreateTrust("B", "C");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageBuildNotification);
            Assert.AreEqual(2, ((PackageBuildNotification)notifications[0]).TrustPackage.Claims.Count);

            var packageID = DB.Packages.First().DatabaseID;
            var trusts = DB.Claims.Select(p => p);
            foreach (var trust in trusts)
            {
                Assert.AreEqual(packageID, trust.PackageDatabaseID);
            }

            foreach (var tp in DB.ClaimPackageRelationships)
            {
                Assert.IsTrue(tp.PackageID == packageID);
                Assert.IsTrue(tp.ClaimID > 0);
            }
        }
    }
}
