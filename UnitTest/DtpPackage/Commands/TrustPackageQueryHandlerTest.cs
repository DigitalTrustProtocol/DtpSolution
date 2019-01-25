using DtpCore.Builders;
using DtpCore.Commands.Packages;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class TrustPackageQueryHandlerTest : StartupMock
    {

        private NotificationSegment CreateClaim(string issuerName, string subjectName)
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
            var result = Mediator.SendAndWait(new PackageQuery());
            
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void One()
        {
            CreateClaim("A", "B");

            var notifications = Mediator.SendAndWait(new BuildPackageCommand());
            var addedPackage = ((PackageBuildNotification)notifications[0]).Package;

            var result = Mediator.SendAndWait(new PackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            var package = result[0];
            TrustDBService.LoadPackageClaims(package);
            Assert.AreEqual(addedPackage.DatabaseID, package.DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, package.Claims.Count);
        }

        [TestMethod]
        public void Two()
        {
            // Create first package
            CreateClaim("A", "B");
            Mediator.SendAndWait(new BuildPackageCommand());

            // Create second package
            CreateClaim("B", "C");
            var notifications = Mediator.SendAndWait(new BuildPackageCommand());
            var addedPackage = ((PackageBuildNotification)notifications[0]).Package;

            var result = Mediator.SendAndWait(new PackageQuery());
            Assert.AreEqual(3, result.Count); // Build package and two customs

            result = Mediator.SendAndWait(new PackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);

            var package = result[0];
            TrustDBService.LoadPackageClaims(package);
            Assert.AreEqual(addedPackage.DatabaseID, package.DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, package.Claims.Count);
        }
    }
}
