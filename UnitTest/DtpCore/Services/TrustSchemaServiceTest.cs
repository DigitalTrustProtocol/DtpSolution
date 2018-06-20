using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Builders;
using UnitTest.DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.DtpCore.Services
{
    [TestClass]
    public class TrustSchemaServiceTest : StartupMock
    {
        [TestMethod]
        public void ValidatePackage()
        {
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver")
                .AddTrustTrue("testissuer1", "testsubject1")
                .AddTrustTrue("testissuer2", "testsubject1")
                .Build()
                .Sign();

            var schemaService = ServiceProvider.GetRequiredService<ITrustSchemaService>();

            var result = schemaService.Validate(builder.Package);

            Console.WriteLine(result.ToString());

            Assert.IsTrue(builder.Package.Trusts.Count > 0);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Warnings.Count);
        }

        [TestMethod]
        public void ValidateTrust()
        {
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver")
                .AddTrustTrue("testissuer1", "testsubject1")
                .Build()
                .Sign();
            
            var schemaService = ServiceProvider.GetRequiredService<ITrustSchemaService>();
            var result = schemaService.Validate(builder.CurrentTrust);

            Console.WriteLine(result.ToString());

            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Warnings.Count);
        }
    }
}
