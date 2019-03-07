using DtpCore.Builders;
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
    public class PackageQueryHandlerTest : StartupMock
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
            var result = Mediator.SendAndWait(new PackagePaginatedListQuery());
            
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void One()
        {
            var buildPackage = CreateAndSavePackage("A", "B");

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            
            Assert.IsNotNull(signedPackage);
            Assert.AreEqual(1, signedPackage.Claims.Count);

            var result = Mediator.SendAndWait(new PackagePaginatedListQuery(signedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            var package = result[0];
            TrustDBService.LoadPackageClaims(package);
            Assert.AreEqual(buildPackage.Claims.Count, package.Claims.Count);
        }

        //[TestMethod]
        //public void Two()
        //{
        //    // Create first package
        //    CreateAndSavePackage("A", "B");
        //    Mediator.SendAndWait(new BuildPackageCommand());

        //    // Create second package
        //    CreateAndSavePackage("B", "C");
        //    var notifications = Mediator.SendAndWait(new BuildPackageCommand());
        //    var addedPackage = ((PackageBuildNotification)notifications[0]).Package;

        //    var result = Mediator.SendAndWait(new PackagePaginatedListQuery());
        //    Assert.AreEqual(3, result.Count); // Build package and two customs

        //    result = Mediator.SendAndWait(new PackagePaginatedListQuery(addedPackage.DatabaseID, true));
        //    Assert.AreEqual(1, result.Count);

        //    var package = result[0];
        //    TrustDBService.LoadPackageClaims(package);
        //    Assert.AreEqual(addedPackage.DatabaseID, package.DatabaseID);
        //    Assert.AreEqual(addedPackage.Claims.Count, package.Claims.Count);
        //}
    }
}
