using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using DtpCore.Extensions;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpCore.Strategy;
using DtpGraphCore.Interfaces;
using DtpGraphCore.Model;
using DtpGraphCore.Services;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using DtpCore.Repository;
using DtpGraphCore.Builders;
using DtpGraphCore.Enumerations;
using UnitTest.DtpCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using DtpCore.Model;
using System.Diagnostics;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class GraphQueryTrustTest : TrustGraphMock
    {
        private const string ClaimType = PackageBuilder.BINARY_TRUST_DTP1;

        //[TestMethod]
        //public void Search1()
        //{
        //    _trustBuilder.AddTrust("A", "B", BinaryTrustTrueAttributes);

        //    _graphTrustService.Add(_trustBuilder.Package);  // Build graph!

        //    var trusts = _trustBuilder.Package.Trusts;
        //    var trust = trusts[0];
        //    Console.WriteLine("Trust object");
        //    Console.WriteLine(JsonConvert.SerializeObject(trusts[0], Formatting.Indented));

        //    var queryBuilder = new QueryRequestBuilder(ClaimType);
        //    queryBuilder.Add(trusts[0].Issuer.Address, trusts[0].Subjects[0]);

        //    Console.WriteLine("Query object");
        //    Console.WriteLine(JsonConvert.SerializeObject(queryBuilder.Query, Formatting.Indented));
        //    var queryContext = _graphQueryService.Execute(queryBuilder.Query);
        //    Assert.AreEqual(queryContext.Results.Trusts.Count, 1, "Should be one result!");

        //    var resultTrust = queryContext.Results.Trusts.First();
        //    Assert.AreEqual(resultTrust.Subjects.Count, 1, "Should be one subject!");

        //    var subject = resultTrust.Subjects.First();
        //    //Assert.IsNotNull(queryContext.Subjects);
        //    Console.WriteLine("Issuer ID: " + JsonConvert.SerializeObject(trusts[0].Issuer.Address));
        //    Console.WriteLine("result ID: " + JsonConvert.SerializeObject(subject.Address) + " : Trust SubjectID: " + JsonConvert.SerializeObject(trusts[0].Subjects[0].Address));

        //    //Assert.AreEqual(issuer.Name, trust.Name);
        //    Assert.IsTrue(subject.Address.Equals(trust.Subjects[0].Address));
        //    //Assert.AreEqual(subject.Claims.ClaimModel.Metadata, ClaimMetadata.Reason);
        //    //Assert.IsTrue(issuer.Subjects[0].Claim.ContainsIgnoreCase("Test"));

        //    Console.WriteLine("Query Result");
        //    Console.WriteLine(JsonConvert.SerializeObject(queryContext.Results, Formatting.Indented));
        //    //PrintResult(result.Nodes, search.GraphService, 1);
        //}

        /// <summary>
        /// 1 Source, 1 targets
        /// </summary>
        [TestMethod]
        public void Source1Target1()
        {
            // Build up
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);
            BuildQuery(queryBuilder, "A", "D");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            PrintJson(context.Results);
            // Verify
            //Assert.AreEqual(4, context.Results.Trusts.Count, $"Should be {4} results!");

            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "B", "E");
            VerfifyResult(context, "C", "D");
            VerfifyResult(context, "E", "D");
        }


        /// <summary>
        /// 1 Source, 1 targets
        /// </summary>
        [TestMethod]
        public void Source1Target1LLeafOnly()
        {
            // Build up
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);
            queryBuilder.Query.Flags |= QueryFlags.LeafsOnly;
            queryBuilder.Query.Flags &= ~QueryFlags.FullTree;
            BuildQuery(queryBuilder, "A", "D");

            // Make sure that QueryFlags are serializeable
            var json = JsonConvert.SerializeObject(queryBuilder.Query, Formatting.Indented);
            Console.WriteLine(json);
            var query = JsonConvert.DeserializeObject<QueryRequest>(json);

            // Execute
            var context = _graphQueryService.Execute(query);

            // Verify
            Assert.AreEqual(2, context.Results.Claims.Count, $"Should be {2} results!");

            VerfifyResult(context, "C", "D");
            VerfifyResult(context, "E", "D");
        }


        /// <summary>
        /// 1 Source, 2 targets
        /// </summary>
        [TestMethod]
        public void Source1Target2()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);

            BuildQuery(queryBuilder, "A", "D");
            BuildQuery(queryBuilder, "A", "F");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            PrintJson(context.Results);
            // Verify
            Assert.AreEqual(context.Results.Claims.Count, 6, $"Should be {6} results!");


            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "B", "E");
            VerfifyResult(context, "B", "F");
            VerfifyResult(context, "C", "D");
            VerfifyResult(context, "E", "D");
        }

        ///// <summary>
        ///// 2 Source, 1 targets
        ///// </summary>
        //[TestMethod]
        //public void Source2Target1()
        //{
        //    ClaimTrustTrueGraph();

        //    var queryBuilder = new QueryRequestBuilder(ClaimType);

        //    BuildQuery(queryBuilder, "A", "D");
        //    BuildQuery(queryBuilder, "F", "D");

        //    // Execute
        //    var context = _graphQueryService.Execute(queryBuilder.Query);

        //    // Verify
        //    Assert.AreEqual(context.Results.Count, 6, $"Should be {6} results!");

        //    VerfifyResult(context, "A", "B");
        //    VerfifyResult(context, "B", "C");
        //    VerfifyResult(context, "C", "D");
        //    VerfifyResult(context, "B", "E");
        //    VerfifyResult(context, "E", "D");

        //    VerfifyResult(context, "F", "G");
        //    VerfifyResult(context, "G", "D");
        //}


        /// <summary>
        /// 1 Source, 1 targets unreachable
        /// </summary>
        [TestMethod]
        public void Source1Target1Unreachable()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);

            BuildQuery(queryBuilder, "A", "Unreach");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            Assert.AreEqual(context.Results.Claims.Count, 0, $"Should be {0} results!");
        }


        /// <summary>
        /// 1 Source, 1 targets unreachable
        /// </summary>
        [TestMethod]
        public void Source1Target1NoTrust()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);

            BuildQuery(queryBuilder, "A", "NoTrustD"); 

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "C", "NoTrustD", BinaryTrustFalseAttributes);
            //VerfifyContext(context, 3);
        }

        /// <summary>
        /// 1 Source, 1 targets unreachable
        /// </summary>
        [TestMethod]
        public void Source1Target1MixTrust()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ClaimType);

            BuildQuery(queryBuilder, "A", "MixD");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "C", "MixD", BinaryTrustTrueAttributes);
            VerfifyResult(context, "B", "E");
            VerfifyResult(context, "E", "MixD", BinaryTrustFalseAttributes);
            //VerfifyContext(context, 4);
        }


    }
}
