using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Builders;
using DtpCore.Model;
using DtpCore.Extensions;
using UnitTest.DtpCore.Extensions;
using Newtonsoft.Json;

namespace UnitTest.DtpCore.Strategy
{
    [TestClass]
    public class JsonConverterTest : StartupMock
    {
        const string JSONdata = @"{
  ""trusts"": [
    {
      ""issuer"": {
        ""type"": ""secp256k1-pkh-pkh"",
        ""address"": ""19wbhSA47bXB9sxWWH2xt8eS7612tW1QbB""
      },
      ""subject"": {
        ""address"": ""1KXfYubz5q4C2ozgrYKE1Sq4V7ANj2x6NE""
      },
      ""type"": {
            ""attribute"" : ""binary"",
            ""group"" : ""trust"",
            ""protocol"" : ""dtp1""
       },
      ""claim"": true
        }
      ]
    }";

        //[TestMethod]
        //public void ObjectToStringConverter()
        //{
        //    var builder = new PackageBuilder();
        //    builder.AddClaimTrue("A", "B");
        //    var json = builder.Serialize(Formatting.Indented);
        //    Assert.IsTrue(json.Contains("\"claim\": true"));
        //    Console.WriteLine(json);

        //    // Test claim string 
        //    var package = JsonConvert.DeserializeObject<Package>(json);
        //    var trust = package.Claims[0];
        //    Assert.IsTrue(trust.Value == builder.CurrentClaim.Value);


        //}

        //[TestMethod]
        //public void TestParse()
        //{
        //    // Test claim string 
        //    var package = JsonConvert.DeserializeObject<Package>(JSONdata);
        //    var trust = package.Claims[0];
        //    Assert.IsTrue(trust.Type.Length > 0);
        //}

        [TestMethod]
        public void SerializeDeserialize()
        {
            var builder = new PackageBuilder();
            var claim = builder.BuildBinaryClaim("testissuer1", "testsubject1", true);
            var data = JsonConvert.SerializeObject(claim, Formatting.Indented);
            Console.WriteLine(data);
            var claim2 = JsonConvert.DeserializeObject<Claim>(data);

            Assert.AreEqual(claim2.Type, claim.Type, "Type");
            Assert.AreEqual(claim2.Issuer.Type, claim.Issuer.Type, "Issuer Type");
            Assert.AreEqual(claim2.Issuer.Id, claim.Issuer.Id, "Issuer Address");
            Assert.IsTrue(claim2.Issuer.Proof.Compare(claim.Issuer.Proof) == 0, "Issuer Signature");

            Assert.AreEqual(claim2.Subject.Type, claim.Subject.Type, "Subject Type");
            Assert.AreEqual(claim2.Subject.Id, claim.Subject.Id, "Subject Address");
            Assert.IsTrue(claim2.Subject.Proof.Compare(claim.Subject.Proof) == 0, "Subject Signature");

            Assert.AreEqual(claim2.Expire, claim.Expire);
            Assert.AreEqual(claim2.Activate, claim.Activate);
            Assert.AreEqual(claim2.Created, claim.Created);
            Assert.AreEqual(claim2.Metadata, claim.Metadata);
            Assert.AreEqual(claim2.Timestamps.Count, claim.Timestamps.Count);
            Assert.AreEqual(claim2.Value, claim.Value, "Claim");
            Assert.AreEqual(claim2.Scope, claim.Scope, "Scope");
        }

    }
}
