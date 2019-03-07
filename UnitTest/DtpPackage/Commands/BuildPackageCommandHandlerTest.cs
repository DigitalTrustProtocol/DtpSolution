using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Commands;
using DtpPackageCore.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UnitTest.DtpCore.Extensions;
using UnitTest.TestData;

namespace UnitTest.DtpPackage.Commands
{
    [TestClass]
    public class BuildPackageCommandHandlerTest : StartupMock
    {

        private Package CreateAndSavePackage(string issuerName, string subjectName, bool value = true)
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver");
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_TRUST_DTP1, PackageBuilder.CreateBinaryTrustAttributes(value)).BuildClaimID();

            var command = new AddPackageCommand(builder.Package);
            Mediator.SendAndWait(command);
            var buildPackage = TrustDBService.GetBuildPackage("");

            return buildPackage;
        }


        [TestMethod]
        public void Empty()
        {
            var package = TestPackage.CreateBinary(0);
            var buildPackage = Mediator.SendAndWait(new BuildPackageCommand(package));
            Assert.IsNull(buildPackage);
        }

        [TestMethod]
        public void One()
        {
            var buildPackage = CreateAndSavePackage("A", "B");

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));

            Assert.IsNotNull(signedPackage);
            Assert.AreEqual(1, signedPackage.Claims.Count);

            buildPackage = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage);
            Assert.AreEqual(0, buildPackage.Claims.Count, "Should be no more claims in build package.");
        }

        [TestMethod]
        public void Two()
        {
            var command1 = CreateAndSavePackage("A", "B");
            var command2 = CreateAndSavePackage("B", "C");

            var buildPackages = TrustDBService.GetBuildPackagesAsync().GetAwaiter().GetResult();
            var buildPackage = buildPackages.FirstOrDefault();

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            Assert.IsNotNull(signedPackage);
            Assert.AreEqual(2, signedPackage.Claims.Count);

            var buildPackage2 = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage2);
            Assert.AreEqual(0, buildPackage2.Claims.Count, "Should be no more claims in build package.");

        }

        [TestMethod]
        public void Replace()
        {
            var command1 = CreateAndSavePackage("A", "B", true); // Create first 
            var command2 = CreateAndSavePackage("A", "B", false); // Override it

            var buildPackages = TrustDBService.GetBuildPackagesAsync().GetAwaiter().GetResult();
            var buildPackage = buildPackages.FirstOrDefault();

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            Assert.IsNotNull(signedPackage);

            Assert.AreEqual(1, signedPackage.Claims.Count, "Should only be one because the first one have been replaced");

            var buildPackage2 = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage2);
            Assert.AreEqual(0, buildPackage2.Claims.Count, "Should be no more claims in build package.");

        }

        [TestMethod]
        public void ReplaceData()
        {
            var builder = new PackageBuilder();
            builder.Load(testPackage);
            var command = new AddPackageCommand(builder.Package);
            Mediator.SendAndWait(command);
            var buildPackage = TrustDBService.GetBuildPackage("twitter.com");

            var signedPackage = Mediator.SendAndWait(new BuildPackageCommand(buildPackage));
            Assert.IsNotNull(signedPackage);
            System.Console.WriteLine(signedPackage.ToString());
            Assert.AreEqual(1, signedPackage.Claims.Count, "Should only be one because the first two has been replaced");

            var buildPackage2 = TrustDBService.GetBuildPackage("");
            TrustDBService.LoadPackageClaims(buildPackage2);
            Assert.AreEqual(0, buildPackage2.Claims.Count, "Should be no more claims in build package.");

        }


        private string testPackage = @"{
  ""algorithm"": ""double256.merkle.dtp1"",
  ""claims"": [
    {
      ""id"": ""XOIqPJZi1CkXf++M81y3mkCO/acnVe2S0pC6+jA+8F4="",
      ""created"": 1550957159,
      ""issuer"": {
        ""type"": ""secp256k1-pkh"",
        ""id"": ""1LRNDp2HfVKc1FsCJwbJwMhtssXL8fdTx2"",
        ""signature"": ""Hwd19kHKuoSCreqXx0FkctpE3t8NTjZtaQT86A4gWqVkbCG9LON6BBassp+1DrSqPTIoTClc+GAuIy5dM81dEaI=""
      },
      ""subject"": {
        ""type"": ""name"",
        ""id"": ""jimmysong""
      },
      ""type"": ""binary.trust.dtp1"",
      ""value"": """",
      ""scope"": ""twitter.com"",
      ""expire"": 1
    },
    {
      ""id"": ""WhH44wXVl7qfqcRPqYdjRCkurkehLATHqeUrvKdWf1o="",
      ""created"": 1550957541,
      ""issuer"": {
        ""type"": ""secp256k1-pkh"",
        ""id"": ""1LRNDp2HfVKc1FsCJwbJwMhtssXL8fdTx2"",
        ""signature"": ""H07JJtGLK/NuoGpxwIoyCXASMS9cTt3CYm6ElVZxhqHGe3NX/jyYaKWOfHKIn5dLSi7zEyQwnGcdKf6WQ4GUYqo=""
      },
      ""subject"": {
        ""type"": ""name"",
        ""id"": ""jimmysong""
      },
      ""type"": ""binary.trust.dtp1"",
      ""value"": ""true"",
      ""scope"": ""twitter.com""
    },
    {
      ""id"": ""AtHw/TJyrdRAKeRR1udODRMu5Co8nPL3bS2MBDLybVA="",
      ""created"": 1550957557,
      ""issuer"": {
        ""type"": ""secp256k1-pkh"",
        ""id"": ""1LRNDp2HfVKc1FsCJwbJwMhtssXL8fdTx2"",
        ""signature"": ""IKx9DG2Bcye9LWY4EOP3/0wzv/4aSObu+bbXhkPMB7KiRrHObaF99krRtDW+hDqrXBBFd0W2bStY3aZQosbv0j8=""
      },
      ""subject"": {
        ""type"": ""name"",
        ""id"": ""jimmysong""
      },
      ""type"": ""binary.trust.dtp1"",
      ""value"": ""false"",
      ""scope"": ""twitter.com""
    }
  ]
  }";
    }
}
