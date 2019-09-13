using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using DtpCore.Strategy;
using NBitcoin.Crypto;
using DtpCore.Extensions;
using NBitcoin.DataEncoders;

namespace UnitTest.DtpCore.Strategy
{
    [TestClass]
    public class DerivationBtcPkhTest : StartupMock
    {
        [TestMethod]
        public void GetAddress()
        {
            var derivationBtcPkh = new DerivationSecp256k1PKH();

            var seed = Encoding.UTF8.GetBytes("Hello");
            var key = derivationBtcPkh.GetKey(seed);

            var addressString = derivationBtcPkh.GetAddress(key);

            Console.WriteLine(addressString);
            Assert.IsNotNull(addressString);
        }

        [TestMethod]
        public void CompileThingAddress()
        {
            var source = "https://www.dr.dk";

            var data = source.ToBytes();
            var hash = Hashes.Hash160(data);
            var prefix = new byte[] { 30 };
            var predixData = prefix.Combine(hash.ToBytes());
            var addressString = Encoders.Base58Check.EncodeData(predixData);

            Console.WriteLine(addressString);
            Assert.AreEqual("D6AQSCAhksDdWfESsGyXRmpxeP1mSt7UPQ", addressString);

        }
    }
}
