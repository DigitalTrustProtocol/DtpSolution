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

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class GraphQueryRatingTest : TrustGraphMock
    {
        private string _claimRatingType = PackageBuilder.RATING_CLAIM_DTP1;

        /// <summary>
        /// 1 Source, 1 targets
        /// 2 Degrees out with the final rating
        /// </summary>
        [TestMethod]
        public void Source1Target1()
        {
            // Build up
            EnsureTestGraph();

            //_graphTrustService.Add(_trustBuilder.Package);

            var queryBuilder = new QueryRequestBuilder(_claimRatingType);
            BuildQuery(queryBuilder, "A", "D"); // Query if "person" have a confimation within A's trust sphere.

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            //Assert.AreEqual(context.Results.Trusts.Count, 3, $"Should be {3} results!");

            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "C", "D", PackageBuilder.RATING_CLAIM_DTP1);

            Assert.AreEqual(3, context.Results.Claims.Count, "There is not the correct number of claims in the query result!");
        }



        /// <summary>
        /// 1 Source, 2 targets
        /// </summary>
        [TestMethod]
        public void Source1Target2()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(_claimRatingType);

            BuildQuery(queryBuilder, "A", "D");
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            //Assert.AreEqual(3, context.Results.Trusts.Count, $"Should be {3} results!");

            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "A", "B", PackageBuilder.RATING_CLAIM_DTP1);
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "C", "D", PackageBuilder.RATING_CLAIM_DTP1);
        }

        ///// <summary>
        ///// 2 Source, 1 targets
        ///// </summary>
        //[TestMethod]
        //public void Source2Target1()
        //{
        //    BuildGraph();

        //    var queryBuilder = new QueryRequestBuilder(ClaimType);

        //    BuildQuery(queryBuilder, "A", "D");
        //    BuildQuery(queryBuilder, "F", "D");

        //    // Execute
        //    var context = _graphQueryService.Execute(queryBuilder.Query);

        //    // Verify
        //    Assert.AreEqual(5, context.Results.Count, $"Should be {5} results!");

        //    VerfifyResult(context, "A", "B");
        //    VerfifyResult(context, "B", "C");
        //    VerfifyResult(context, "C", "D", ClaimRating);

        //    VerfifyResult(context, "F", "G");
        //    VerfifyResult(context, "G", "D", ClaimRating);
        //}


        /// <summary>
        /// 1 Source, 1 targets unreachable
        /// </summary>
        [TestMethod]
        public void Source1Target1Unreachable()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(_claimRatingType);

            BuildQuery(queryBuilder, "A", "Unreach");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            Assert.AreEqual(context.Results.Claims.Count, 0, $"Should be {0} results!");
        }


        //private void EnsureTestGraph()
        //{
        //    _trustBuilder.AddTrust("A", "B", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("B", "C", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("C", "D", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("B", "E", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("E", "D", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("B", "F", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("F", "G", ClaimTrustTrue);
        //    _trustBuilder.AddTrust("G", "D", ClaimTrustTrue); // Long way, no trust
        //    _trustBuilder.AddTrust("G", "Unreach", ClaimTrustTrue); // Long way, no trust

        //    _trustBuilder.AddTrust("A", "B", base.ClaimRating);
        //    _trustBuilder.AddTrust("C", "D", base.ClaimRating);
        //    _trustBuilder.AddTrust("G", "D", base.ClaimRating);

        //    _graphTrustService.Add(_trustBuilder.Package);
        //}
    }
}
