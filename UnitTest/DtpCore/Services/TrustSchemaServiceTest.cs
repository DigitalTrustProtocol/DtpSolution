using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Builders;
using UnitTest.DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace UnitTest.DtpCore.Services
{
    [TestClass]
    public class TrustSchemaServiceTest : StartupMock
    {
        [TestMethod]
        public void GetTrustTypeString()
        {
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver")
                .AddTrustTrue("testissuer1", "testsubject1")
                .AddTrustTrue("testissuer2", "testsubject1")
                .Build()
                .Sign();

            var schemaService = ServiceProvider.GetRequiredService<ITrustSchemaService>();
            var trust = builder.Package.Trusts[0];
            Assert.IsTrue(trust.Type == schemaService.GetTrustTypeString(trust));

            var trustType = schemaService.GetTrustTypeObject(trust);
            trust.Type = JsonConvert.SerializeObject(trustType);

            var result = schemaService.GetTrustTypeString(trust);
            Assert.IsTrue(result == TrustBuilder.BINARY_TRUST_DTP1);
        }


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
