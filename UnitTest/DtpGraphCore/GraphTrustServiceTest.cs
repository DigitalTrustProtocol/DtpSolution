using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using System;
using System.Linq;
using DtpCore.Extensions;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpCore.Strategy;
using DtpGraphCore.Interfaces;
using DtpGraphCore.Model;
using DtpGraphCore.Services;
using DtpGraphCore.Enumerations;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class GraphTrustServiceTest : StartupMock
    {

        [TestMethod]
        public void NodeIndex()
        {
            var trustDerivationService = new TrustDerivationService();
            var graphModelService = new GraphTrustService();

            var keyA = trustDerivationService.GetAddressFromPassword("A");
            var keyB = trustDerivationService.GetAddressFromPassword("B");

            graphModelService.EnsureGraphIssuer(keyA);
            graphModelService.EnsureGraphIssuer(keyB);

            Assert.IsTrue(graphModelService.Graph.IssuerIndex.ContainsKey(keyA));
            Assert.IsTrue(graphModelService.Graph.IssuerIndex.ContainsKey(keyB));
            Assert.IsTrue(graphModelService.Graph.IssuerIndex[keyA] == 0);
            Assert.IsTrue(graphModelService.Graph.IssuerIndex[keyB] == 1);
        }

        //[TestMethod]
        //public void BuildEdge1()
        //{
        //    var graphTrustService = new GraphTrustService();

        //    var trustBuilder = new TrustBuilder(ServiceProvider);
        //    var trust = trustBuilder.AddTrust("A", "B", TrustBuilder.CreateFollowClaim()).Package.Trusts[0];

        //    trust.Claims[0].Activate = (uint)DateTime.Now.UnixTimestamp() - 1000;
        //    trust.Claims[0].Expire = (uint)DateTime.Now.UnixTimestamp() + 1000;
        //    trust.Claims[0].Cost = 112;

        //    graphTrustService.Add(trustBuilder.Package);
        //    var graph = graphTrustService.Graph;
        //    Assert.IsTrue(graph.Issuers.Count == 2);

        //    Assert.IsTrue(graph.Issuers[0].Subjects.Count == 1);
        //    Assert.IsTrue(graph.Issuers[0].Subjects.First().Value.TargetIssuer.Index == 1);
        //    Assert.IsTrue(graph.Issuers[0].Subjects.First().Value.Claims.Count == 1);
        //    //Assert.IsTrue(graph.Issuers[0].Subjects.First().Value.Claims.First().Value.Data.Types == ClaimType.Trust);

        //    Assert.IsTrue(graph.IssuerIndex.Count == 2);
        //    Assert.IsTrue(graph.SubjectTypes.Count() == 2);
        //    Assert.IsTrue(graph.SubjectTypes.ContainsKey("person"));
        //    Assert.IsTrue(graph.Scopes.Count() == 1);
        //    Assert.IsTrue(graph.Scopes.ContainsKey(""));
        //}

        //[TestMethod]
        //public void BuildEdge2()
        //{
        //    var graphModelService = new GraphModelService();
        //    var graphTrustService = new GraphTrustService(graphModelService);
        //    var trustBuilder = new TrustBuilder(ServiceProvider);

        //    trustBuilder.AddTrust("A", "B", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("B", "C", TrustBuilder.CreateTrustTrue());
        //    var trusts = trustBuilder.Package.Trusts;

        //    Assert.IsTrue(trusts[0].Subjects[0].Address.Compare(trusts[1].Issuer.Address) == 0);

        //    graphTrustService.Add(trustBuilder.Package);

        //    Assert.IsTrue(graphModelService.Graph.Issuer.Count == 3);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects.Length == 1);
        //    //Assert.AreEqual(graphModelService.Graph.Issuers[0].Subjects[0].NameIndex, 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].SubjectId == 1);

        //    //Assert.AreEqual(graphModelService.Graph.Issuers[1].Subjects[0].NameIndex, 2);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.IssuersIndex.Count == 3);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.Count == 2);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.ContainsKey("person"));
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.Count == 1);
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.ContainsKey(""));
        //}


        //[TestMethod]
        //public void BuildEdge3()
        //{
        //    var graphModelService = new GraphModelService();
        //    var graphTrustService = new GraphTrustService(graphModelService);
        //    var trustBuilder = new TrustBuilder(ServiceProvider);

        //    trustBuilder.AddTrust("A", "B", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("B", "C", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("C", "A", TrustBuilder.CreateTrustTrue());
        //    var trusts = trustBuilder.Package.Trusts;

        //    Assert.IsTrue(trusts[0].Subjects[0].Address.Compare(trusts[1].Issuer.Address) == 0);

        //    graphTrustService.Add(trustBuilder.Package);

        //    Assert.IsTrue(graphModelService.Graph.Issuer.Count == 3);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects.Length == 1);
        //    //Assert.AreEqual(graphModelService.Graph.Issuers[0].Subjects[0].NameIndex, 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].SubjectId == 1);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.IssuersIndex.Count == 3);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.Count == 2);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.ContainsKey("person"));
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.Count == 1);
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.ContainsKey(""));

        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].SubjectId == 0);
        //}

        //[TestMethod]
        //public void BuildEdge4()
        //{
        //    var graphModelService = new GraphModelService();
        //    var graphTrustService = new GraphTrustService(graphModelService);
        //    var trustBuilder = new TrustBuilder(ServiceProvider);

        //    trustBuilder.AddTrust("A", "B", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("A", "C", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("B", "C", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("C", "A", TrustBuilder.CreateTrustTrue());

        //    var trusts = trustBuilder.Package.Trusts;

        //    graphTrustService.Add(trustBuilder.Package);

        //    Assert.IsTrue(graphModelService.Graph.Issuer.Count == 3);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects.Length == 2);
        //    //Assert.AreEqual(graphModelService.Graph.Issuers[0].Subjects[0].NameIndex, 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].SubjectId == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[1].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[1].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].SubjectId == 0);

        //    Assert.IsTrue(graphModelService.Graph.IssuersIndex.Count == 3);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.Count == 2);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.ContainsKey("person"));
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.Count == 1);
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.ContainsKey(""));

        //}


        //[TestMethod]
        //public void BuildEdge5()
        //{
        //    var graphModelService = new GraphModelService();
        //    var graphTrustService = new GraphTrustService(graphModelService);
        //    var trustBuilder = new TrustBuilder(ServiceProvider);

        //    trustBuilder.AddTrust("A", "B", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("A", "C", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("B", "C", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("C", "A", TrustBuilder.CreateTrustTrue());
        //    trustBuilder.AddTrust("D", "E", TrustBuilder.CreateTrustTrue());

        //    var trusts = trustBuilder.Package.Trusts;

        //    graphTrustService.Add(trustBuilder.Package);

        //    Assert.IsTrue(graphModelService.Graph.Issuer.Count == 5);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects.Length == 2);
        //    //Assert.AreEqual(graphModelService.Graph.Issuers[0].Subjects[0].NameIndex, 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[0].SubjectId == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[1].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[0].Subjects[1].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[1].Subjects[0].SubjectId == 2);

        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects.Length == 1);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].Claim.Flags == ClaimType.Trust);
        //    Assert.IsTrue(graphModelService.Graph.Issuer[2].Subjects[0].SubjectId == 0);

        //    Assert.IsTrue(graphModelService.Graph.IssuersIndex.Count == 5);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.Count == 2);
        //    Assert.IsTrue(graphModelService.Graph.SubjectTypesIndex.ContainsKey("person"));
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.Count == 1);
        //    Assert.IsTrue(graphModelService.Graph.ScopeIndex.ContainsKey(""));
        //}
    }
}
