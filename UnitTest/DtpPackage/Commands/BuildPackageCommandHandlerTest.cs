using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using UnitTest.TestData;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class BuildPackageCommandHandlerTest : StartupMock
    {

        private Package CreateAndSavePackage(string issuerName, string subjectName)
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver");
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_TRUST_DTP1, PackageBuilder.CreateBinaryTrustAttributes(true)).BuildClaimID();

            var command = new AddPackageCommand(builder.Package);
            Mediator.SendAndWait(command);
            var buildPackage = TrustDBService.GetBuildPackage("");

            return buildPackage;
        }
        [TestMethod]
        public void Empty()
        {
            var package = TestPackage.CreateBinary(0);
            var buildPackage = Mediator.SendAndWait(new BuildPackageCommand(package));
            Assert.IsNull(buildPackage);
        }

        [TestMethod]
        public void One()
        {
            var buildPackage = CreateAndSavePackage("A", "B");

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            Assert.IsNotNull(signedPackage);
            Assert.AreEqual(1, signedPackage.Claims.Count);

            buildPackage = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage);
            Assert.AreEqual(0, buildPackage.Claims.Count, "Should be no more claims in build package.");
        }

        [TestMethod]
        public void Two()
        {
            var command1 = CreateAndSavePackage("A", "B");
            var command2 = CreateAndSavePackage("B", "C");

            var buildPackages = TrustDBService.GetBuildPackagesAsync().GetAwaiter().GetResult();
            var buildPackage = buildPackages.FirstOrDefault();

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            Assert.IsNotNull(signedPackage);
            Assert.AreEqual(2, signedPackage.Claims.Count);

            var buildPackage2 = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage2);
            Assert.AreEqual(0, buildPackage2.Claims.Count, "Should be no more claims in build package.");

        }
    }
}
