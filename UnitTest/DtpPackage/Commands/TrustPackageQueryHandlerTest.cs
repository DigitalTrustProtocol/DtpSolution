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

        private Claim CreateClaim(string issuer, string subject)
        {
            var builder = new PackageBuilder();
            var claim = builder.BuildBinaryClaim(issuer, subject, true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = claim, Package = builder.Package });
            DB.SaveChanges();
            return claim;
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
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, result[0].Claims.Count);
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
            Assert.AreEqual(2, result.Count);

            result = Mediator.SendAndWait(new PackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Claims.Count, result[0].Claims.Count);
        }
    }
}
