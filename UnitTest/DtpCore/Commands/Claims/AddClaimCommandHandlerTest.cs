using DtpCore.Builders;
using DtpCore.Commands.Packages;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpCore.Commands.Trusts
{
    [TestClass]
    public class AddClaimCommandHandlerTest : StartupMock
    {

        private Claim CreateClaim()
        {
            var builder = new PackageBuilder(ServiceProvider);
            var claim = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = claim });
            DB.SaveChanges();
            return claim;
        }

        [TestMethod]
        public void Add()
        {
            var builder = new PackageBuilder(ServiceProvider);
            var trust = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);
            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust });
            Assert.AreEqual(1, result.Count);
            var last = result.Last();
            Assert.IsTrue(last is ClaimAddedNotification);
        }

        [TestMethod]
        public void Replace()
        {
            var oldtrust = CreateClaim();

            var builder = new PackageBuilder(ServiceProvider);
            var trust = builder.BuildBinaryClaim("testissuer1", "testsubject1", false);

            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust });

            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(result[0] is ClaimReplacedNotification);
            Assert.IsTrue(((ClaimReplacedNotification)result[0]).Claim.Id == oldtrust.Id);

            Assert.IsTrue(result[1] is ClaimAddedNotification);
            Assert.IsTrue(((ClaimAddedNotification)result[1]).Claim.Id == trust.Id);
        }

        [TestMethod]
        public void Exist()
        {
            var trust = CreateClaim();

            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = trust });

            Assert.AreEqual(1, result.Count);

            Assert.IsTrue(result[0] is ClaimExistNotification);
        }

        [TestMethod]
        public void Old()
        {
            var newtrust = CreateClaim();

            var builder = new PackageBuilder(ServiceProvider);
            var oldtrust = builder.BuildBinaryClaim("testissuer1", "testsubject1", true, 1); 

            NotificationSegment result = Mediator.SendAndWait(new AddClaimCommand { Claim = oldtrust });

            Assert.AreEqual(1, result.Count);

            Assert.IsTrue(result[0] is ClaimObsoleteNotification);
        }


    }
}
