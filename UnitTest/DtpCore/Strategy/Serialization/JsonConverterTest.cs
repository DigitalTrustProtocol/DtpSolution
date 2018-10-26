using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DtpCore.Builders;
using DtpCore.Model;
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
            var builder = new TrustBuilder(ServiceProvider);
            builder.AddTrustTrue("A", "B");
            var json = builder.Serialize(Formatting.Indented);
            Assert.IsTrue(json.Contains("\"claim\": true"));
            Console.WriteLine(json);

            // Test claim string 
            var package = JsonConvert.DeserializeObject<Package>(json);
            var trust = package.Trusts[0];
            Assert.IsTrue(trust.Claim == builder.CurrentTrust.Claim);


        }

        [TestMethod]
        public void TestParse()
        {
            // Test claim string 
            var package = JsonConvert.DeserializeObject<Package>(JSONdata);
            var trust = package.Trusts[0];
            Assert.IsTrue(trust.Type.Length > 0);
        }

    }
}
