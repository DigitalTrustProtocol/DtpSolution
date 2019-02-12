using DtpCore.Extensions;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTest.TestData;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class FetchPackageCommandHandlerTest : StartupMock
    {
        [TestMethod]
        public void One()
        {
            var package = TestPackage.CreateBinary(1);

            var storeNotifications = Mediator.SendAndWait(new StorePackageCommand(package));
            Assert.IsTrue(storeNotifications[0] is PackageStoredNotification);

            var notification = storeNotifications.FindLast<PackageStoredNotification>();
            var packageB = Mediator.SendAndWait(new FetchPackageCommand(notification.Message));

            //Assert.AreEqual(3, notifications.Count, "There should be one notifications");

            //Assert.IsTrue(notifications[0] is ClaimAddedNotification);
            //Assert.IsTrue(notifications[1] is PackageAddedNotification);
            Assert.IsNotNull(packageB);
        }
    }

}

