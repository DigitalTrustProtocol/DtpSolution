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

        private NotificationSegment CreateTrust(string issuerName, string subjectName)
        {
             var builder = new PackageBuilder();
            builder.SetServer("testserver");
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_TRUST_DTP1, PackageBuilder.CreateBinaryTrustAttributes(true)).BuildClaimID();
            
            NotificationSegment result = Mediator.SendAndWait(new AddPackageCommand { Package = builder.Package });
            return result;
        }

        [TestMethod]
        public void Empty()
        {
            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageNoClaimsNotification);
        }

        [TestMethod]
        public void One()
        {
            CreateTrust("A", "B");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageBuildNotification);
            Assert.AreEqual(1, ((PackageBuildNotification)notifications[0]).Package.Claims.Count);

            var buildPackage = TrustDBService.GetBuildPackage();
            TrustDBService.LoadPackageClaims(buildPackage);
            Assert.AreEqual(0, buildPackage.Claims.Count, "Should be no more claims in build package.");
        }

        [TestMethod]
        public void Two()
        {
            CreateTrust("A", "B");
            CreateTrust("B", "C");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());

            Assert.AreEqual(1, notifications.Count, "There should be one notifications");
            Assert.IsTrue(notifications[0] is PackageBuildNotification);
            Assert.AreEqual(2, ((PackageBuildNotification)notifications[0]).Package.Claims.Count);

            var buildPackage = TrustDBService.GetBuildPackage();
            TrustDBService.LoadPackageClaims(buildPackage);
            Assert.AreEqual(0, buildPackage.Claims.Count, "Should be no more claims in build package.");

        }
    }
}
