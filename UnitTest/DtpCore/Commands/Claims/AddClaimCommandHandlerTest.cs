using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpCore.Commands.Trusts
{
    [TestClass]
    public class AddClaimCommandHandlerTest : StartupMock
    {

        private Claim CreateAndAddClaim()
        {
            var builder = new PackageBuilder();
            var claim = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);
            //NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = claim });
            NotificationSegment result = Mediator.SendAndWait(new AddPackageCommand(builder.Package));
            DB.SaveChanges();
            return claim;
        }

        private Package CreateBanPackage()
        {
            var builder = new PackageBuilder();
            var claim = builder.BuildClaim(PackageBuilder.REMOVE_CLAIMS_DTP1, "testissuer1", "", true);
            return builder.Package;
        }

        [TestMethod]
        public void Add()
        {
            var builder = new PackageBuilder();
            var trust = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust, Package = builder.Package });
            Assert.AreEqual(1, result.Count);
            var last = result.Last();
            Assert.IsTrue(last is ClaimAddedNotification);
        }

        [TestMethod]
        public void Replace()
        {
            var oldtrust = CreateAndAddClaim();

            var builder = new PackageBuilder();
            var trust = builder.BuildBinaryClaim("testissuer1", "testsubject1", false);

            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust, Package = builder.Package });

            //Assert.AreEqual(2, result.Count);

            //Assert.IsTrue(result[0] is ClaimReplacedNotification);
            //Assert.IsTrue(((ClaimReplacedNotification)result[0]).Claim.Id == oldtrust.Id);

            //Assert.IsTrue(result[1] is ClaimAddedNotification);
            //Assert.IsTrue(((ClaimAddedNotification)result[1]).Claim.Id == trust.Id);
        }

        [TestMethod]
        public void Exist()
        {
            var claim = CreateAndAddClaim();

            var builder = new PackageBuilder();
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = claim, Package = builder.Package });

            //Assert.AreEqual(1, result.Count);

            //Assert.IsTrue(result[0] is ClaimExistNotification, "Wrong type: " + result[0].GetType().Name);
        }


        [TestMethod]
        public void Ban()
        {
            var claim = CreateAndAddClaim(); // Create and add to DB
            Assert.AreEqual(1, DB.Claims.Count());
            Assert.AreEqual(1, DB.Packages.Count());
            Assert.AreEqual(1, DB.ClaimPackageRelationships.Count());

            var ban = CreateBanPackage();

            NotificationSegment result = Mediator.SendAndWait(new AddPackageCommand(ban));

            Assert.AreEqual(1, DB.Claims.Count());
            Assert.AreEqual(1, DB.Packages.Count(), "There should be packages for deleted claims remaining."); 
            Assert.AreEqual(1, DB.ClaimPackageRelationships.Count(),"There should not be old references remaining."); 
        }



        [TestMethod]
        public void Old()
        {
            Assert.AreEqual(0, DB.Claims.Count());
            Assert.AreEqual(0, DB.Packages.Count());
            Assert.AreEqual(0, DB.ClaimPackageRelationships.Count());


            var newtrust = CreateAndAddClaim();

            var builder = new PackageBuilder();
            var oldtrust = builder.BuildBinaryClaim("testissuer1", "testsubject1", true, 1); 

            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = oldtrust, Package = builder.Package });

            //Assert.AreEqual(1, result.Count);

            //Assert.IsTrue(result[0] is ClaimObsoleteNotification, "Wrong type: "+ result[0].GetType().Name);
        }


    }
}
