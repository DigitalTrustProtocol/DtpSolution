using DtpCore.Extensions;
using DtpGraphCore.Model;
using DtpPackageCore.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class GraphLoadSaveServiceTest : TrustGraphMock
    {
        [TestMethod]
        public void LoadFromDatabase()
        {
            // Setup
            BuildTestGraph();
            var package = _trustBuilder.Package;

            var result = Mediator.SendAndWait(new AddPackageCommand( _trustBuilder.Package));

            // Load into Graph
            _graphLoadSaveService.LoadFromDatabase();

            // Test Graph
            Assert.IsTrue(_graphTrustService.Graph.Issuers.Count > 0, $"Missing issuers in Graph.");
            Assert.IsTrue(_graphTrustService.Graph.Claims.Count > 0, $"Missing Claims in Graph.");
        }


        [TestMethod]
        public void LoadFromMemory()
        {
            // Setup
            BuildTestGraph();
            var package = _trustBuilder.Package;

            _graphTrustService.Add(package);


            var source = _graphTrustService.Graph;

            string data = _graphTrustService.Graph.SerializeObject();

            var target = data.DeserializeObject<GraphModel>();


            Assert.AreEqual(source.Claims.Count, target.Claims.Count, "Claims count are not equal");
            Assert.AreEqual(source.ClaimIndex.Count, target.ClaimIndex.Count, "ClaimIndex count are not equal");
            Assert.AreEqual(source.ClaimAttributes.Count(), target.ClaimAttributes.Count(), "ClaimAttributes count are not equal");
            Assert.AreEqual(source.ClaimType.Count(), target.ClaimType.Count(), "ClaimType count are not equal");
            Assert.AreEqual(source.Issuers.Count, target.Issuers.Count, "Issuers count are not equal");
            Assert.AreEqual(source.IssuerIndex.Count, target.IssuerIndex.Count, "IssuerIndex count are not equal");
            Assert.AreEqual(source.Scopes.Count(), target.Scopes.Count(), "Scopes count are not equal");
        }
    }
}
