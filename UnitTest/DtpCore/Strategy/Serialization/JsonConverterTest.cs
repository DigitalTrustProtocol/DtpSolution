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
        ""type"": ""btc-pkh"",
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

        [TestMethod]
        public void ObjectToStringConverter()
        {
            var builder = new PackageBuilder(ServiceProvider);
            builder.AddTrustTrue("A", "B");
            var json = builder.Serialize(Formatting.Indented);
            Assert.IsTrue(json.Contains("\"claim\": true"));
            Console.WriteLine(json);

            // Test claim string 
            var package = JsonConvert.DeserializeObject<Package>(json);
            var trust = package.Claims[0];
            Assert.IsTrue(trust.Value == builder.CurrentClaim.Value);


        }

        [TestMethod]
        public void TestParse()
        {
            // Test claim string 
            var package = JsonConvert.DeserializeObject<Package>(JSONdata);
            var trust = package.Claims[0];
            Assert.IsTrue(trust.Type.Length > 0);
        }

        [TestMethod]
        public void SerializeDeserialize()
        {
            var builder = new PackageBuilder(ServiceProvider);
            var trust = builder.BuildBinaryTrust("testissuer1", "testsubject1", true);
            var data = JsonConvert.SerializeObject(trust, Formatting.Indented);
            Console.WriteLine(data);
            var trust2 = JsonConvert.DeserializeObject<Claim>(data);

            Assert.AreEqual(trust2.Type, trust.Type, "Type");
            Assert.AreEqual(trust2.Algorithm, trust.Algorithm);
            Assert.AreEqual(trust2.Issuer.Type, trust.Issuer.Type, "Issuer Type");
            Assert.AreEqual(trust2.Issuer.Id, trust.Issuer.Id, "Issuer Address");
            Assert.IsTrue(trust2.Issuer.Signature.Compare(trust.Issuer.Signature) == 0, "Issuer Signature");

            Assert.AreEqual(trust2.Subject.Type, trust.Subject.Type, "Subject Type");
            Assert.AreEqual(trust2.Subject.Id, trust.Subject.Id, "Subject Address");
            Assert.IsTrue(trust2.Subject.Signature.Compare(trust.Subject.Signature) == 0, "Subject Signature");

            Assert.AreEqual(trust2.Expire, trust.Expire);
            Assert.AreEqual(trust2.Activate, trust.Activate);
            Assert.AreEqual(trust2.Created, trust.Created);
            Assert.AreEqual(trust2.Note, trust.Note);
            Assert.AreEqual(trust2.Timestamps.Count, trust.Timestamps.Count);
            Assert.AreEqual(trust2.Value, trust.Value, "Claim");
            Assert.AreEqual(trust2.Scope, trust.Scope, "Scope");
        }

    }
}
