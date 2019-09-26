using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using DtpServer.Controllers;
using DtpCore.Builders;
using DtpCore.Model;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;
using System.Collections.Generic;
using DtpGraphCore.Builders;
using DtpCore.Service;

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
            var result = (ObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            Assert.IsNotNull(result);

            //var httpResult = (HttpResult)result.Value;
            //Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : "+ httpResult.Data);

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

        [TestMethod]
        public void LargeQuery()
        {
            // Setup
            var maxPeers = 30;
            var maxDegrees = 2;
            var issuerPeer = "A";
            var counter = 0;
            //var _packageController = ServiceProvider.GetRequiredService<PackageController>();
            Claim lastClaim = null;
            var lastSubjectName = string.Empty;
            // Test Add and schema validation

            addPeer(issuerPeer, maxPeers, 0, maxDegrees, (issuer, subject) =>
            {
                var b = new PackageBuilder().SetServer("testserver");
                b.AddClaimTrue(issuer, subject);
                lastClaim = b.CurrentClaim;
                lastSubjectName = subject;
                _graphTrustService.Add(b.Package);
                //var result = (ObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
                counter++;

            });


            Console.WriteLine("Inserted Claims: "+ counter);
            //Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));


            var queryBuilder = new QueryRequestBuilder(PackageBuilder.BINARY_CLAIM_DTP1);
            BuildQuery(queryBuilder, issuerPeer, lastSubjectName);

            QueryContext context = null;
            using(new TimeMe("Query"))
            {
                context = _graphQueryService.Execute(queryBuilder.Query);
            }
            PrintJson("Claims found : "+ context.Results.Claims.Count);



            //var _queryController = ServiceProvider.GetRequiredService<QueryController>();
            //var query = new QueryRequest();
            //query.Issuer = new Identity();
            //query.Issuer.Id = PackageBuilderExtensions.GetAddress(issuerPeer);
            //query.Subjects = new List<string> { lastClaim.Subject.Id };
            //query.Types = new List<string> { "binarytrust" };
            //query.Scope = "";
            //query.Flags = QueryFlags.FullTree;



            //var result = _queryController.ResolvePost(query);

            //Assert.IsNotNull(result);

            //Console.WriteLine("Query Claims: " + result.Value.Results.Claims.Count);



            //Assert.IsNotNull(result);


            //var lastClaim = b.Package.Claims.LastOrDefault();


            //_trustBuilder.SetServer("testserver")
            //    .AddClaimTrue("A", "B")
            //    .AddClaimTrue("B", "C")
            //    .AddClaimTrue("C", "D")

            //    .Build().Sign();

            //Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            //var _packageController = ServiceProvider.GetRequiredService<PackageController>();
            //// Test Add and schema validation
            //var result = (ObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
            //Assert.IsNotNull(result);

            ////var httpResult = (HttpResult)result.Value;
            ////Assert.AreEqual(HttpResultStatusType.Success.ToString(), httpResult.Status, httpResult.Message + " : "+ httpResult.Data);

            //// Check db
            //Assert.AreEqual(3, _trustDBService.Claims.Count(), $"Should be {3} Trusts");

            //// Test Graph
            //var _queryController = ServiceProvider.GetRequiredService<QueryController>();
            ////result = (OkObjectResult)_queryController.Get(TrustBuilderExtensions.GetAddress("A"), TrustBuilderExtensions.GetAddress("D"), QueryFlags.LeafsOnly);

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

        private void addPeer(string issuerPeer, int count, int degree, int maxDegree, Action<string,string> addP)
        {
            for (int i = 0; i < count; i++)
            {
                var subjectPeer = $"{Guid.NewGuid().ToString()}{degree}:{i}";
                addP(issuerPeer, subjectPeer);
                if (degree < maxDegree)
                    addPeer(subjectPeer, count, degree + 1, maxDegree, addP);
            }
        }
    }
}
