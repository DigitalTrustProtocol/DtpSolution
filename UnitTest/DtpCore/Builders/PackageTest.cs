using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Text;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Factories;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpCore.Strategy;
using UnitTest.DtpCore.Extensions;
using DtpCore.Model;

namespace UnitTest.DtpCore.Builders
{
    [TestClass]
    public class PackageTest : StartupMock
    {
        [TestMethod]
        public void Build()
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver");
            var claim = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);

            builder.Build();
            builder.Sign();

            var package = builder.Package;
            //var schemaService = ServiceProvider.GetRequiredService<ITrustSchemaService>();

            //schemaService = new TrustSchemaService(ServiceProvider);

            //var result = schemaService.Validate(builder.Package);

            //Console.WriteLine(result.ToString());

            //Assert.IsTrue(builder.Package.Trusts.Count > 0);
            //Assert.AreEqual(0, result.Errors.Count);
            //Assert.AreEqual(0, result.Warnings.Count);

            var content = JsonConvert.SerializeObject(builder.Package, Formatting.Indented);
            Console.WriteLine(content);
            var dePackage = JsonConvert.DeserializeObject<Package>(content);
            Assert.AreEqual(package.Claims.Count, dePackage.Claims.Count);
        }
    }
}
