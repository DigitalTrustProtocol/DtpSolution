using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpGraphCore.Interfaces;
using UnitTest.DtpCore.Extensions;
using Newtonsoft.Json;
using System;
using DtpGraphCore.Builders;
using System.Collections.Generic;
using DtpGraphCore.Model;
using DtpCore.Model;
using DtpGraphCore.Extensions;
using DtpGraphCore.Controllers;

namespace UnitTest.DtpGraphCore
{
    public class TrustGraphMock : StartupMock
    {
        protected IGraphTrustService _graphTrustService { get; set; }

        protected TrustBuilder _trustBuilder { get; set; } 

        protected ITrustDBService _trustDBService { get; set; } 

        protected IGraphQueryService _graphQueryService { get; set; } 

        protected TrustController _trustController { get; set; } 

        protected IGraphLoadSaveService _graphLoadSaveService { get; set; } 

        protected string BinaryTrustTrueAttributes { get; set; } 

        protected string BinaryTrustFalseAttributes { get; set; } 

        protected string ConfirmAttributes { get; set; }

        protected string RatingAtrributes { get; set; } 

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _graphTrustService = ServiceProvider.GetRequiredService<IGraphTrustService>();
            _trustBuilder = new TrustBuilder(ServiceProvider);
            _trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            //_graphQueryService = new GraphQueryService(_graphTrustService);
            _graphQueryService = ServiceProvider.GetRequiredService<IGraphQueryService>();
            _trustController = ServiceProvider.GetRequiredService<TrustController>();
            _graphLoadSaveService = ServiceProvider.GetRequiredService<IGraphLoadSaveService>();

            BinaryTrustTrueAttributes = TrustBuilder.CreateBinaryTrustAttributes(true);
            BinaryTrustFalseAttributes = TrustBuilder.CreateBinaryTrustAttributes(false);
            ConfirmAttributes = TrustBuilder.CreateConfirmAttributes();
            RatingAtrributes = TrustBuilder.CreateRatingAttributes(100);
        }


        protected void PrintJson(object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Console.WriteLine(json);
        }

        protected QueryRequest BuildQuery(QueryRequestBuilder queryBuilder, string source, string target)
        {
            var sourceAddress = TrustBuilderExtensions.GetAddress(source);
            var subject = new Identity
            {
                Address = TrustBuilderExtensions.GetAddress(target)
            };
            queryBuilder.Add(sourceAddress, subject.Address);

            return queryBuilder.Query;
        }
        protected void VerfifyContext(QueryContext context, int exspectedResults, int exspcetedErrors = 0)
        {
            Assert.AreEqual(exspcetedErrors, context.Errors.Count, $"{string.Join("\r\n", context.Errors.ToArray())}");
            Assert.AreEqual(exspectedResults, context.Results.Trusts.Count, $"Should be {exspectedResults} results!");

        }

        protected void VerfifyResult(QueryContext context, string source, string target, string type = "")
        {
            var sourceAddress = TrustBuilderExtensions.GetAddress(source);
            var targetAddress = TrustBuilderExtensions.GetAddress(target);
            var sourceIndex = _graphTrustService.Graph.IssuerIndex.GetValueOrDefault(sourceAddress);
            var targetIndex = _graphTrustService.Graph.IssuerIndex.GetValueOrDefault(targetAddress);

            var tracker = context.TrackerResults.GetValueOrDefault(sourceIndex);
            Assert.IsNotNull(tracker, $"Result is missing source: {source}");

            var subject = tracker.Subjects.GetValueOrDefault(targetIndex);
            Assert.IsNotNull(subject, $"Result is missing for subject for: {source} - subject: {target}");

            //if (trustClaim != null)
            //{
            //    var graphClaim = _graphTrustService.CreateGraphClaim(trustClaim);
            //    var exist = subject.Claims.Exist(graphClaim.Scope, graphClaim.Type);
            //    Assert.IsTrue(exist, "Subject missing the claim type: " + trustClaim.Type);
            //}
        }

        protected void BuildTestGraph()
        {
            _trustBuilder.SetServer("testserver");

            _trustBuilder.AddTrust("A", "B", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "C", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("C", "D", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "E", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("E", "D", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("B", "F", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("F", "G", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("G", "D", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes); // Long way, no trust
            _trustBuilder.AddTrust("G", "Unreach", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes); // Long way, no trust

            _trustBuilder.AddTrust("A", "B", TrustBuilder.CONFIRMTRUST_TC1, ConfirmAttributes);
            _trustBuilder.AddTrust("C", "D", TrustBuilder.CONFIRMTRUST_TC1, ConfirmAttributes);
            _trustBuilder.AddTrust("G", "D", TrustBuilder.CONFIRMTRUST_TC1, ConfirmAttributes);

            _trustBuilder.AddTrust("A", "B", TrustBuilder.RATING_TC1, RatingAtrributes);
            _trustBuilder.AddTrust("C", "D", TrustBuilder.RATING_TC1, RatingAtrributes);
            _trustBuilder.AddTrust("G", "D", TrustBuilder.RATING_TC1, RatingAtrributes);

            _trustBuilder.AddTrust("A", "NoTrustB", TrustBuilder.BINARYTRUST_TC1, BinaryTrustFalseAttributes);
            _trustBuilder.AddTrust("B", "NoTrustC", TrustBuilder.BINARYTRUST_TC1, BinaryTrustFalseAttributes);
            _trustBuilder.AddTrust("C", "NoTrustD", TrustBuilder.BINARYTRUST_TC1, BinaryTrustFalseAttributes);

            _trustBuilder.AddTrust("C", "MixD", TrustBuilder.BINARYTRUST_TC1, BinaryTrustTrueAttributes);
            _trustBuilder.AddTrust("E", "MixD", TrustBuilder.BINARYTRUST_TC1, BinaryTrustFalseAttributes);

            _trustBuilder.Build().Sign();
        }

        protected void EnsureTestGraph()
        {
            BuildTestGraph();
            _graphTrustService.Add(_trustBuilder.Package);

        }

    }
}
