﻿using DtpCore.Builders;
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
    public class TrustPackageQueryHandlerTest : StartupMock
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
            var result = Mediator.SendAndWait(new TrustPackageQuery());
            
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void One()
        {
            CreateTrust("A", "B");

            var notifications = Mediator.SendAndWait(new BuildTrustPackageCommand());
            var addedPackage = ((TrustPackageBuildNotification)notifications[0]).TrustPackage;

            var result = Mediator.SendAndWait(new TrustPackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Trusts.Count, result[0].Trusts.Count);
        }

        [TestMethod]
        public void Two()
        {
            // Create first package
            CreateTrust("A", "B");
            Mediator.SendAndWait(new BuildTrustPackageCommand());

            // Create second package
            CreateTrust("B", "C");
            var notifications = Mediator.SendAndWait(new BuildTrustPackageCommand());
            var addedPackage = ((TrustPackageBuildNotification)notifications[0]).TrustPackage;

            var result = Mediator.SendAndWait(new TrustPackageQuery());
            Assert.AreEqual(2, result.Count);

            result = Mediator.SendAndWait(new TrustPackageQuery(addedPackage.DatabaseID, true));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(addedPackage.DatabaseID, result[0].DatabaseID);
            Assert.AreEqual(addedPackage.Trusts.Count, result[0].Trusts.Count);
        }
    }
}