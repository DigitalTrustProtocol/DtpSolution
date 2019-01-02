using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Model;
using DtpGraphCore.Builders;
using DtpGraphCore.Controllers;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;
using UnitTest.DtpCore.Extensions;
using DtpServer.Controllers;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class QueryControllerTest : TrustGraphMock
    {
        [TestMethod]
        public void AddAndQuery1()
        {
            // Setup

            _trustBuilder.SetServer("testserver")
                .AddTrustTrue("A", "B")
                .AddTrustTrue("B", "C")
                .AddTrustTrue("C", "D")
                .Build().Sign();

            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            var _trustController = ServiceProvider.GetRequiredService<TrustController>();
            // Test Add and schema validation
            var result = (OkObjectResult)_trustController.Add(_trustBuilder.Package);
            Assert.IsNotNull(result);

            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : "+ httpResult.Data);

            // Check db
            Assert.AreEqual(3, _trustDBService.Trusts.Count(), $"Should be {3} Trusts");

            // Test Graph
            var _queryController = ServiceProvider.GetRequiredService<QueryController>();
            result = (OkObjectResult)_queryController.Get(TrustBuilderExtensions.GetAddress("A"), TrustBuilderExtensions.GetAddress("D"), QueryFlags.LeafsOnly);

            Assert.IsNotNull(result);

            httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);
            Console.WriteLine("Result:-------------------------");
            PrintJson(httpResult);

            var context = (QueryContext)httpResult.Data;

            // Verify
            Assert.AreEqual(1, context.Results.Claims.Count, $"Should be {1} results!");

            VerfifyResult(context, "C", "D");
        }
    }
}
