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
    public class GraphQueryConfirmTest : TrustGraphMock
    {
        private const string ConfirmClaimType = TrustBuilder.CONFIRMTRUST_TC1;


        /// <summary>
        /// 1 Source, 1 targets
        /// </summary>
        [TestMethod]
        public void Source1Target1()
        {
            // Build up
            //BuildGraph();
            _trustBuilder.AddTrust("A", "B", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));
            _trustBuilder.AddTrust("B", "C", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));

            _trustBuilder.AddTrust("B", "C", TrustBuilder.CONFIRMTRUST_TC1, TrustBuilder.CreateConfirmAttributes(true));


            _graphTrustService.Add(_trustBuilder.Package);

            var queryBuilder = new QueryRequestBuilder(ConfirmClaimType);
            BuildQuery(queryBuilder, "A", "C"); // Query if "person" have a confimation within A's trust sphere.

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            //VerfifyContext(context, 2);

            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "B", "C", ConfirmAttributes);
        }



        /// <summary>
        /// 1 Source, 2 targets
        /// </summary>
        [TestMethod]
        public void Source1Target2()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ConfirmClaimType);

            BuildQuery(queryBuilder, "A", "D");
            BuildQuery(queryBuilder, "A", "B");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            //VerfifyContext(context, 3);

            VerfifyResult(context, "A", "B");
            VerfifyResult(context, "A", "B", ConfirmAttributes);
            VerfifyResult(context, "B", "C");
            VerfifyResult(context, "C", "D", ConfirmAttributes);
        }

        ///// <summary>
        ///// 2 Source, 1 targets
        ///// </summary>
        //[TestMethod]
        //public void Source2Target1()
        //{
        //    BuildGraph();

        //    var queryBuilder = new QueryRequestBuilder(ConfirmClaimType);

        //    BuildQuery(queryBuilder, "A", "D");
        //    BuildQuery(queryBuilder, "F", "D");

        //    // Execute
        //    var context = _graphQueryService.Execute(queryBuilder.Query);

        //    // Verify
        //    VerfifyResult(context, "A", "B");
        //    VerfifyResult(context, "B", "C");
        //    VerfifyResult(context, "C", "D", ClaimConfirmTrue);

        //    VerfifyResult(context, "F", "G");
        //    VerfifyResult(context, "G", "D", ClaimConfirmTrue);

        //    VerfifyContext(context, 5);
        //}


        /// <summary>
        /// 1 Source, 1 targets unreachable
        /// </summary>
        [TestMethod]
        public void Source1Target1Unreachable()
        {
            EnsureTestGraph();

            var queryBuilder = new QueryRequestBuilder(ConfirmClaimType);

            BuildQuery(queryBuilder, "A", "Unreach");

            // Execute
            var context = _graphQueryService.Execute(queryBuilder.Query);

            // Verify
            VerfifyContext(context, 0,0);
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

        //    _trustBuilder.AddTrust("A", "B", ClaimConfirmTrue);
        //    _trustBuilder.AddTrust("C", "D", ClaimConfirmTrue);
        //    _trustBuilder.AddTrust("G", "D", ClaimConfirmTrue);

        //    _trustBuilder.AddTrust("A", "B", ClaimRating);
        //    _trustBuilder.AddTrust("C", "D", ClaimRating);
        //    _trustBuilder.AddTrust("G", "D", ClaimRating);

        //    _graphTrustService.Add(_trustBuilder.Package);
        //}
    }
}
