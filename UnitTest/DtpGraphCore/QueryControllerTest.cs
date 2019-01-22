using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using DtpCore.Enumerations;
using DtpCore.Model;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;
using UnitTest.DtpCore.Extensions;
using DtpServer.Controllers;
using DtpCore.Controllers;

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
                .AddClaimTrue("A", "B")
                .AddClaimTrue("B", "C")
                .AddClaimTrue("C", "D")
                .Build().Sign();

            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            var _packageController = ServiceProvider.GetRequiredService<PackageController>();
            // Test Add and schema validation
            var result = (OkObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            Assert.IsNotNull(result);

            var httpResult = (HttpResult)result.Value;
            Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : "+ httpResult.Data);

            // Check db
            Assert.AreEqual(3, _trustDBService.Claims.Count(), $"Should be {3} Trusts");

            // Test Graph
            var _queryController = ServiceProvider.GetRequiredService<QueryController>();
            //result = (OkObjectResult)_queryController.Get(TrustBuilderExtensions.GetAddress("A"), TrustBuilderExtensions.GetAddress("D"), QueryFlags.LeafsOnly);

            //Assert.IsNotNull(result);

            //httpResult = (HttpResult)result.Value;
            //Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : " + httpResult.Data);
            //Console.WriteLine("Result:-------------------------");
            //PrintJson(httpResult);

            //var context = (QueryContext)httpResult.Data;

            //// Verify
            //Assert.AreEqual(1, context.Results.Claims.Count, $"Should be {1} results!");

            //VerfifyResult(context, "C", "D");
        }
    }
}
