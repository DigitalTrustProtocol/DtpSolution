using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Builders;
using UnitTest.DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace UnitTest.DtpCore.Model.Schema
{
    [TestClass]
    public class PackageSchemaValidatorTest : StartupMock
    {
        [TestMethod]
        public void GetTrustTypeString()
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver")
                .AddClaimTrue("testissuer1", "testsubject1")
                .AddClaimTrue("testissuer2", "testsubject1")
                .Build()
                .Sign();

            var schemaService = ServiceProvider.GetRequiredService<IPackageSchemaValidator>();
            var claim = builder.Package.Claims[0];
            Assert.IsTrue(claim.Type == schemaService.GetTrustTypeString(claim));

            var trustType = schemaService.GetTrustTypeObject(claim);
            claim.Type = JsonConvert.SerializeObject(trustType);

            var result = schemaService.GetTrustTypeString(claim);
            Assert.IsTrue(result == PackageBuilder.BINARY_TRUST_DTP1);
        }


        [TestMethod]
        public void ValidatePackage()
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver")
                .AddClaimTrue("testissuer1", "testsubject1")
                .AddClaimTrue("testissuer2", "testsubject1")
                .Build()
                .Sign();

            var schemaService = ServiceProvider.GetRequiredService<IPackageSchemaValidator>();

            var result = schemaService.Validate(builder.Package);

            Console.WriteLine(result.ToString());

            Assert.IsTrue(builder.Package.Claims.Count > 0);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Warnings.Count);
        }

        [TestMethod]
        public void ValidateTrust()
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver")
                .AddClaimTrue("testissuer1", "testsubject1")
                .Build()
                .Sign();
            
            var schemaService = ServiceProvider.GetRequiredService<IPackageSchemaValidator>();
            var result = schemaService.Validate(builder.CurrentClaim);

            Console.WriteLine(result.ToString());

            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Warnings.Count);
        }
    }
}
