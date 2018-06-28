using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpGraphCore.Builders;
using DtpGraphCore.Controllers;
using DtpGraphCore.Enumerations;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class TrustControllerTest : TrustGraphMock
    {
        [TestMethod]
        public void Add()
        {
            // Setup
            EnsureTestGraph();

            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            // Test Add and schema validation
            var result = (OkObjectResult)_trustController.Add(_trustBuilder.Package);
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
            var result = (OkObjectResult)_trustController.Add(_trustBuilder.Package);
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("A", "B", TrustBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            builder.Build().Sign();

            result = (OkObjectResult)_trustController.Add(builder.Package);
            httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            // Test Graph
            var queryBuilder = new QueryRequestBuilder(TrustBuilder.BINARY_TRUST_DTP1);
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            var trust = context.Results.Trusts[0];

            VerfifyResult(context, "A", "B");
            Assert.AreEqual(BinaryTrustFalseAttributes, trust.Claim, $"Attributes are wrong!");
        }

        [TestMethod]
        public void AddAndRemove()
        {
            // Setup
            EnsureTestGraph();
            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            // Test Add and schema validation
            var result = (OkObjectResult)_trustController.Add(_trustBuilder.Package);
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("A", "B", TrustBuilder.BINARY_TRUST_DTP1, BinaryTrustFalseAttributes);
            builder.CurrentTrust.Expire = 1; // Remove the trust from Graph!
            builder.Build().Sign();

            result = (OkObjectResult)_trustController.Add(builder.Package);
            httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            // Test Graph
            var queryBuilder = new QueryRequestBuilder(TrustBuilder.BINARY_TRUST_DTP1);
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            Assert.AreEqual(0, context.Results.Trusts.Count(), $"Should be no trusts!");
        }

        [TestMethod]
        public void AddWithTimestamp()
        {
            // Setup
            EnsureTestGraph();

            // Test Add and schema validation
            var result = (OkObjectResult)_trustController.Add(_trustBuilder.Package);
            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);

            var okResult = (OkObjectResult)_trustController.Get(_trustBuilder.CurrentTrust.Id);
            var trust = (Trust)((HttpResult)okResult.Value).Data;
            Assert.IsTrue(trust.Timestamps.Count > 0, "Missing timestamp entry in trust");

        }

    }
}
