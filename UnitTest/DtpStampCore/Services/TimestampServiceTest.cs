using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DtpCore.Extensions;
using DtpStampCore.Interfaces;
using System.Text;
using DtpCore.Strategy;
using DtpCore.Model;

namespace UnitTest.DtpStampCore.Services
{
    [TestClass]
    public class TimestampServiceTest : StartupMock
    {
        [TestMethod]
        public void GetMerkleRoot()
        {
            var timestampService = ServiceProvider.GetRequiredService<ITimestampService>();

            var hashAlgorithm = new Double256();
            var merkle = new MerkleTreeSorted(hashAlgorithm);

            var one = Encoding.UTF8.GetBytes("Hello world\n");
            var oneHash = hashAlgorithm.HashOf(one);
            var timestamp = new Timestamp { Source = oneHash };
            var oneProof = merkle.Add(timestamp);
            var merkleRoot = merkle.Build();

            var timestampMerkleRoot = timestampService.GetMerkleRoot(timestamp);
            Assert.IsTrue(ByteExtensions.Compare(timestampMerkleRoot, merkleRoot.Hash) == 0, $"Calculated timestamp merkle root {timestampMerkleRoot.ToHex()} are not equal to source {merkleRoot.Hash.ToHex()}");
        }
    }
}
