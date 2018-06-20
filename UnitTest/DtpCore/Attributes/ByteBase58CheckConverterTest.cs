using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using DtpCore.Builders;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpCore.Attributes
{
    [TestClass]
    public class ByteBase58CheckConverterTest: StartupMock
    {
        //[TestMethod]
        //public void Serialize()
        //{
        //    var name = "A";
        //    var builder = new TrustBuilder(ServiceProvider);
        //    builder.SetServer("testserver");
        //    builder.AddTrust(name, "B", TrustBuilder.BINARYTRUST_TC1, TrustBuilder.CreateBinaryTrustAttributes(true));
        //    builder.Build().Sign();

        //    var serialized = builder.ToString();

        //    var issuerKey = TrustBuilderExtensions.ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
        //    var nameAddress = TrustBuilderExtensions.ScriptService.StringifyAddress(issuerKey);

        //    Console.Write(serialized);
        //    Assert.IsTrue(serialized.Contains(nameAddress), "Did not find the serialized base58Check address in the serialized packaged");
            
        //}

        //[TestMethod]
        //public void Deserialize()
        //{

        //}
    }
}
