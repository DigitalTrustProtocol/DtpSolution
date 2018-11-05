﻿using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Commands.Trusts;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using UnitTest.DtpPackage.Mocks;

namespace UnitTest.DtpCore.Commands.Trusts
{
    [TestClass]
    public class AddTrustCommandHandlerTest : StartupMock
    {

        private Trust CreateTrust()
        {
            var builder = new TrustBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true);
            NotificationsResult result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });
            DB.SaveChanges();
            return trust;
        }

        [TestMethod]
        public void Add()
        {
            var builder = new TrustBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true);
            NotificationsResult result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });
            Assert.AreEqual(1, result.Count);
            var last = result.Last();
            Assert.IsTrue(last is TrustAddedNotification);
        }

        [TestMethod]
        public void Replace()
        {
            var oldtrust = CreateTrust();

            var builder = new TrustBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", false);

            NotificationsResult result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });

            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(result[0] is TrustReplacedNotification);
            Assert.IsTrue(((TrustReplacedNotification)result[0]).Trust.Id == oldtrust.Id);

            Assert.IsTrue(result[1] is TrustAddedNotification);
            Assert.IsTrue(((TrustAddedNotification)result[1]).Trust.Id == trust.Id);
        }

        [TestMethod]
        public void Exist()
        {
            var trust = CreateTrust();

            NotificationsResult result = Mediator.SendAndWait(new AddTrustCommand { Trust = trust });

            Assert.AreEqual(1, result.Count);

            Assert.IsTrue(result[0] is TrustExistNotification);
        }

        [TestMethod]
        public void Old()
        {
            var newtrust = CreateTrust();

            var builder = new TrustBuilder(ServiceProvider);
            var oldtrust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true, 1); 

            NotificationsResult result = Mediator.SendAndWait(new AddTrustCommand { Trust = oldtrust });

            Assert.AreEqual(1, result.Count);

            Assert.IsTrue(result[0] is TrustObsoleteNotification);
        }


    }
}
