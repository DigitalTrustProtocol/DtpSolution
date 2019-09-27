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
using DtpServer.Controllers;

namespace UnitTest.DtpGraphCore
{
    public class TrustGraphMock : StartupMock
    {
        protected IGraphClaimService _graphTrustService { get; set; }

        protected PackageBuilder _trustBuilder { get; set; } 

        protected ITrustDBService _trustDBService { get; set; } 

        protected IGraphQueryService _graphQueryService { get; set; } 

        //protected TrustController _trustController { get; set; } 
        protected PackageController _packageController { get; set; }

        protected IGraphLoadSaveService _graphLoadSaveService { get; set; } 

        protected string BinaryTrustTrueAttributes { get; set; } 

        protected string BinaryTrustFalseAttributes { get; set; } 

        protected string ConfirmAttributes { get; set; }

        protected string RatingAtrributes { get; set; } 

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _graphTrustService = ServiceProvider.GetRequiredService<IGraphClaimService>();
            _trustBuilder = new PackageBuilder();
            _trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            //_graphQueryService = new GraphQueryService(_graphTrustService);
            _graphQueryService = ServiceProvider.GetRequiredService<IGraphQueryService>();
            //_trustController = ServiceProvider.GetRequiredService<TrustController>();
            _packageController = ServiceProvider.GetRequiredService<PackageController>();
            _graphLoadSaveService = ServiceProvider.GetRequiredService<IGraphLoadSaveService>();

            BinaryTrustTrueAttributes = PackageBuilder.CreateBinaryTrustAttributes(true);
            BinaryTrustFalseAttributes = PackageBuilder.CreateBinaryTrustAttributes(false);
            ConfirmAttributes = PackageBuilder.CreateConfirmAttributes();
            RatingAtrributes = PackageBuilder.CreateRatingAttributes(5);
        }


        protected void PrintJson(object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Console.WriteLine(json);
        }

        protected QueryRequest BuildQuery(QueryRequestBuilder queryBuilder, string source, string target)
        {
            var sourceAddress = PackageBuilderExtensions.GetAddress(source);
            var subject = new Identity
            {
                Id = PackageBuilderExtensions.GetAddress(target)
            };
            queryBuilder.Add(sourceAddress, subject.Id);

            return queryBuilder.Query;
        }
        protected void VerfifyContext(QueryContext context, int exspectedResults, int exspcetedErrors = 0)
        {
            Assert.AreEqual(exspcetedErrors, context.Errors.Count, $"{string.Join("\r\n", context.Errors.ToArray())}");
            Assert.AreEqual(exspectedResults, context.Results.Claims.Count, $"Should be {exspectedResults} results!");

        }

        protected void VerfifyResult(QueryContext context, string source, string target, string type = "")
        {
            var sourceAddress = PackageBuilderExtensions.GetAddress(source);
            var targetAddress = PackageBuilderExtensions.GetAddress(target);
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

            _trustBuilder.AddClaim("A", "B", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("B", "C", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("C", "D", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("B", "E", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("E", "D", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("B", "F", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("F", "G", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("G", "D", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes); // Long way, no trust
            _trustBuilder.AddClaim("G", "Unreach", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes); // Long way, no trust

            _trustBuilder.AddClaim("A", "B", PackageBuilder.CONFIRM_CLAIM_DTP1, ConfirmAttributes);
            _trustBuilder.AddClaim("C", "D", PackageBuilder.CONFIRM_CLAIM_DTP1, ConfirmAttributes);
            _trustBuilder.AddClaim("G", "D", PackageBuilder.CONFIRM_CLAIM_DTP1, ConfirmAttributes);

            _trustBuilder.AddClaim("A", "B", PackageBuilder.RATING_CLAIM_DTP1, RatingAtrributes);
            _trustBuilder.AddClaim("C", "D", PackageBuilder.RATING_CLAIM_DTP1, RatingAtrributes);
            _trustBuilder.AddClaim("G", "D", PackageBuilder.RATING_CLAIM_DTP1, RatingAtrributes);

            _trustBuilder.AddClaim("A", "NoTrustB", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustFalseAttributes);
            _trustBuilder.AddClaim("B", "NoTrustC", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustFalseAttributes);
            _trustBuilder.AddClaim("C", "NoTrustD", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustFalseAttributes);

            _trustBuilder.AddClaim("C", "MixD", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustTrueAttributes);
            _trustBuilder.AddClaim("E", "MixD", PackageBuilder.BINARY_CLAIM_DTP1, BinaryTrustFalseAttributes);

            _trustBuilder.Build().Sign();
        }

        protected void EnsureTestGraph()
        {
            BuildTestGraph();
            _graphTrustService.Add(_trustBuilder.Package);

        }

    }
}
