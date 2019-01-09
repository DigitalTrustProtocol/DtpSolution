using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Model;
using DtpGraphCore.Builders;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class PackageControllerTest : TrustGraphMock
    {
        [TestMethod]
        public void Add()
        {
            // Setup
            EnsureTestGraph();

            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            // Test Add and schema validation
            var result = (OkObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : "+ httpResult.Data);

            // Check db
            //Assert.AreEqual(3, _trustDBService.Trusts.Count(), $"Should be {3} Trusts");
            //Assert.AreEqual(3, _trustDBService.Subjects.Count(), $"Should be {3} Trusts");
            //Assert.AreEqual(3, _trustDBService.DBContext.Claims.Count(), "Wrong number of Claims");


            //// Test Graph
            //var queryBuilder = new QueryRequestBuilder(ClaimTrustTrue.Type);
            //queryBuilder.Query.Flags |= QueryFlags.LeafsOnly;
            //BuildQuery(queryBuilder, "A", "D");

            //// Execute
            //var context = _graphQueryService.Execute(queryBuilder.Query);

            //// Verify
            //Assert.AreEqual(1, context.Results.Count, $"Should be {1} results!");

            //VerfifyResult(context, "C", "D");
        }


        [TestMethod]
        public void AddAndUpdate()
        {
            // Setup
            EnsureTestGraph();
            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            // Test Add and schema validation
            var result = (OkObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            var builder = new PackageBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("A", "B", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            builder.Build().Sign();

            result = (OkObjectResult)_packageController.PostPackage(builder.Package).GetAwaiter().GetResult();
            httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            // Test Graph
            var queryBuilder = new QueryRequestBuilder(PackageBuilder.BINARY_TRUST_DTP1);
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            var trust = context.Results.Claims[0];

            VerfifyResult(context, "A", "B");
            Assert.AreEqual(BinaryTrustFalseAttributes, trust.Value, $"Attributes are wrong!");
        }

        [TestMethod]
        public void AddAndRemove()
        {
            // Setup
            EnsureTestGraph();
            Console.WriteLine(_trustBuilder.Package.ToString());
            // Test Add and schema validation
            var result = (OkObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            var builder = new PackageBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("A", "B", PackageBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            builder.CurrentClaim.Expire = 1; // Remove the trust from Graph!
            builder.Build().Sign();

            result = (OkObjectResult)_packageController.PostPackage(builder.Package).GetAwaiter().GetResult();
            httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            // Test Graph
            var queryBuilder = new QueryRequestBuilder(PackageBuilder.BINARY_TRUST_DTP1);
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            Assert.AreEqual(0, context.Results.Claims.Count(), $"Should be no trusts!");
        }

        [TestMethod]
        public void AddWithTimestamp()
        {
            // Setup
            EnsureTestGraph();

            // Test Add and schema validation
            var result = (OkObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            //var okResult = (OkObjectResult)_packageController.Get(_trustBuilder.CurrentClaim.Id);
            //var trust = (Claim)((HttpResult)okResult.Value).Data;
            //Assert.IsTrue(trust.Timestamps.Count > 0, "Missing timestamp entry in trust");

        }

    }
}
