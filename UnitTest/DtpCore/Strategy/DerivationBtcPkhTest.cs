using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using DtpCore.Strategy;

namespace UnitTest.DtpCore.Strategy
{
    [TestClass]
    public class DerivationBtcPkhTest : StartupMock
    {
        [TestMethod]
        public void StringifyAddress()
        {
            var derivationBtcPkh = new DerivationBTCPKH();

            var seed = Encoding.UTF8.GetBytes("Hello");
            var key = derivationBtcPkh.GetKey(seed);

            var addressString = derivationBtcPkh.StringifyAddress(key);

            Console.WriteLine(addressString);
            Assert.IsNotNull(addressString);
        }
    }
}
