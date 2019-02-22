using DtpCore.Extensions;
using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UnitTest.TestData;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class PublishPackageCommandHandlerTest : StartupMock
    {
        [TestMethod]
        public void Publish()
        {
            var package = TestPackage.CreateBinary(3);
            package.File = "ipfs://"+Guid.NewGuid().ToString();

            var message = Mediator.SendAndWait(new PublishPackageCommand(package));
            Assert.IsNotNull(message);

            // Validate message
            var validator = ServiceProvider.GetRequiredService<IPackageMessageValidator>();
            Assert.IsTrue(validator.Validate(message, out IList<string> errors), string.Join(",", errors));
        }
    }
}
