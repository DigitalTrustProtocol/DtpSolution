﻿using DtpCore.Commands.Packages;
using DtpCore.Extensions;
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

            var result = Mediator.SendAndWait(new AddPackageCommand { Package = _trustBuilder.Package });

            // Load into Graph
            _graphLoadSaveService.LoadFromDatabase();

            // Test Graph
            Assert.IsTrue(_graphTrustService.Graph.Issuers.Count > 0, $"Missing issuers in Graph.");
            Assert.IsTrue(_graphTrustService.Graph.Claims.Count > 0, $"Missing Claims in Graph.");
        }
    }
}
