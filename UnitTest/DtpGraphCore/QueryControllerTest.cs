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
using System.IO;

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
            Console.WriteLine("Memory on test start: " + AutoSize(GC.GetTotalMemory(true)));

            // Setup
            var maxPeers = 20;
            var maxDegrees = 1;
            var issuerPeer = "A";
            var counter = 0;
            //var _packageController = ServiceProvider.GetRequiredService<PackageController>();
            Claim lastClaim = null;
            var lastSubjectName = string.Empty;
            // Test Add and schema validation
            var b = new PackageBuilder().SetServer("testserver");

            using (new TimeMe("Build"))
            {
                addPeer(issuerPeer, maxPeers, 0, maxDegrees, (issuer, subject) =>
                {
                    b.AddClaimTrue(issuer, subject);
                    b.AddClaimRating(issuer, subject, 5);
                    lastClaim = b.CurrentClaim;
                    lastSubjectName = subject;
                    //var result = (ObjectResult)_packageController.PostPackage(_trustBuilder.Package).GetAwaiter().GetResult();
                    counter++;

                });

                Console.WriteLine("Memory after package build: " + AutoSize(GC.GetTotalMemory(true)));


            }

            using (new TimeMe("Add Claims"))
            {
                _graphTrustService.Add(b.Package);

                b.Package = null;
                GC.Collect();
            }
            Console.WriteLine("Memory after graph build: " + AutoSize(GC.GetTotalMemory(true)));

            Console.WriteLine("Inserted Claims: " + counter);
            //Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));


            var queryBuilder = new QueryRequestBuilder(PackageBuilder.BINARY_CLAIM_DTP1);
            BuildQuery(queryBuilder, issuerPeer, lastSubjectName);
            QueryContext context = null;
            using (new TimeMe("Query 1"))
            {
                context = _graphQueryService.Execute(queryBuilder.Query);
            }
            using (new TimeMe("Query 2"))
            {
                context = _graphQueryService.Execute(queryBuilder.Query);
            }
            using (new TimeMe("Query 3"))
            {
                context = _graphQueryService.Execute(queryBuilder.Query);
            }
            PrintJson("Claims found : " + context.Results.Claims.Count);


            Console.WriteLine("-----------------------------------------------");
            context = null;

            var data = string.Empty;
            using (new TimeMe("Serialize"))
            {
                data = _graphTrustService.JsonSerialize();
                var path = Path.Combine(Directory.GetCurrentDirectory(), "graph.json");
                Console.WriteLine("Saving graph data to : " + path);
                File.WriteAllText(path, data);
            }

            Console.WriteLine("Data size : " + AutoSize(data.Length));
            _graphTrustService.Graph = null;
            GC.Collect();
            Console.WriteLine("Memory after graph null: " + AutoSize(GC.GetTotalMemory(true)));

            using (new TimeMe("Deserialize"))
            {
                _graphTrustService.JsonDeserialize(data);
            }
            Console.WriteLine("Memory after serialize graph build: " + AutoSize(GC.GetTotalMemory(true)));

            Console.WriteLine("Memory before data null: " + AutoSize(GC.GetTotalMemory(true)));

            data = null;
            GC.Collect();
            Console.WriteLine("Memory after data null: " + AutoSize(GC.GetTotalMemory(true)));


            using (new TimeMe("Re Query"))
            {
                context = _graphQueryService.Execute(queryBuilder.Query);
            }
            PrintJson("Re Claims found : " + context.Results.Claims.Count);
        }

        //[TestMethod]
        //public void LoadLargeDataSize()
        //{
        //    var startMem = GC.GetTotalMemory(true);
        //    Console.WriteLine("Memory on test start: " + AutoSize(startMem));

        //    var path = Path.Combine(Directory.GetCurrentDirectory(), "graph.json");

        //    using (new TimeMe("Load and Deserialize"))
        //    {
        //        // deserialize JSON directly from a file
        //        using (StreamReader file = File.OpenText(path))
        //        {
        //            JsonSerializer serializer = new JsonSerializer();

        //            _graphTrustService.Graph = (GraphModel)serializer.Deserialize(file, typeof(GraphModel));
        //        }
        //    }
        //    //var claimsCounter = 27930;
        //    var g = _graphTrustService.Graph;
        //    g.Issuers.TrimExcess();

        //    var claimsCounter = g.IssuerIndex.Count;           


        //    Console.WriteLine("IssuerIndex count found : " + _graphTrustService.Graph.IssuerIndex.Count);

        //    var endMem = GC.GetTotalMemory(true);
        //    var memSize = endMem - startMem;
        //    var memPerClaim = memSize / claimsCounter;

        //    Console.WriteLine("Memory on test end: " + AutoSize(memSize));
        //    Console.WriteLine("Memory per Claim: " + AutoSize(memPerClaim));
        //}

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

        public string AutoSize(long number)
        {
            double tmp = number;
            string suffix = " B ";
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " KB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " MB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " GB"; }
            if (tmp > 1024) { tmp = tmp / 1024; suffix = " TB"; }
            return tmp.ToString("n") + suffix;
        }
    }
}
