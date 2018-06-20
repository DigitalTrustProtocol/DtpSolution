using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;


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

            // Add to db
            var addResult = _trustDBService.Add(package);

            Assert.IsTrue(addResult, "Error adding package to Graph");
            Assert.AreEqual(package.Trusts.Count(), _trustDBService.Trusts.Count(), $"Should be {package.Trusts.Count()} Trusts");

            // Load into Graph
            _graphLoadSaveService.LoadFromDatabase();

            // Test Graph
            Assert.IsTrue(_graphTrustService.Graph.Issuers.Count > 0, $"Missing issuers in Graph.");
            Assert.IsTrue(_graphTrustService.Graph.Claims.Count > 0, $"Missing Claims in Graph.");
        }
    }
}
