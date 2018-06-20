using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using System;
using System.Text;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpStampCore.Extensions;
using DtpStampCore.Interfaces;
using DtpStampCore.Repository;
using DtpStampCore.Services;

namespace UnitTest.DtpStampCore.Services
{
    [TestClass]
    public class BlockchainServiceTest : StartupMock
    {
        public const string FundingKeyWIF = "cMcGZkth7ufvQC59NSTSCTpepSxXbig9JfhCYJtn9RppU4DXx4cy";

        [TestMethod]
        public void VerifyFunds()
        {
            var blockchainService = ServiceProvider.GetRequiredService<IBlockchainService>();

            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);

            var key = new Key(fundingKey);
            var address = key.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine(address.ToString());
            
            var result = blockchainService.VerifyFunds(fundingKey, null);
            Assert.AreEqual(0, result, "Missing funds on address: " + address);
        }

        [TestMethod]
        public void AddressTimestampedConfirmed()
        {
            var blockchainService = ServiceProvider.GetRequiredService<IBlockchainService>();

            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);

            var key = new Key(fundingKey);
            var address = key.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine(address.ToString());

            var result = blockchainService.GetTimestamp(fundingKey);
            Assert.IsTrue(result.Confirmations > 0, "No confirmations on: " + address);

            Console.WriteLine($"Number of confirmations: {result.Confirmations}");
            Console.WriteLine($"Address time: {DatetimeExtensions.FromUnixTime(result.Time)}");
            
        }

        [TestMethod]
        public void AddressTimestampedUnknown()
        {
            //var blockchainService = ServiceProvider.GetRequiredService<IBlockchainService>();
            var config = ServiceProvider.GetRequiredService<IConfiguration>();
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var repo = new SoChainTransactionRepository(config);
            var blockchainService = new BitcoinService(repo, derivationStrategyFactory);

            var key = new Key(blockchainService.DerivationStrategy.HashOf(Guid.NewGuid().ToByteArray())); // A random key
            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(key.ToString(Network.TestNet));

            var address = key.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine(address.ToString());

            var result = blockchainService.GetTimestamp(fundingKey);
            Assert.IsTrue(result.Confirmations == -1, "Should have been -1 (no unconfired and no confirmations): " + address);
        }


        //[TestMethod] // Live test - be carefull
        public void Send()
        {
            //var BitcoinService
            //var blockchainService = ServiceProvider.GetRequiredService<IBlockchainService>();
            var config = ServiceProvider.GetRequiredService<IConfiguration>();
            var derivationStrategyFactory = ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            var repo = new SoChainTransactionRepository(config);
            var blockchainService = new BitcoinService(repo, derivationStrategyFactory); 
            
            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);

            var key = new Key(fundingKey);
            var address = key.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine(address.ToString());

            var merkleRoot = Encoding.UTF8.GetBytes("Hello world!"); // address: mgXcnc8q2PBQFt9r1KyZgdGcP8F4pjcjRd
            var merkleRootKey = new Key(blockchainService.DerivationStrategy.GetKey(merkleRoot));
            var merkleRootAddress = merkleRootKey.PubKey.GetAddress(Network.TestNet);
            Console.WriteLine($"Merkle Root Address: {merkleRootAddress}");
            
            var txOut = blockchainService.Send(merkleRoot, fundingKey);
            Assert.IsTrue(txOut.Count > 0, "No tx out on send");

            var firstTx = new Transaction(txOut[0]);
            
            Console.WriteLine($"{firstTx.GetHash().ToString()}");

            Console.WriteLine($"First: {txOut[0].ConvertToHex()} and second: {txOut[1].ConvertToHex()}");
        }


        [TestMethod]
        public void TxData()
        {
            var blockchainService = ServiceProvider.GetRequiredService<IBlockchainService>();

            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);
            var key = new Key(fundingKey);
            var address = key.PubKey.GetAddress(Network.Main);
            Console.WriteLine(address.ToString());

            var data = "0100000001968848192b8beebedb1b39865971276fcb27c8f82a5821bf626954a468dc8b8e0100000000ffffffff01455aa400000000001976a9145648ae02ecfdc282f301a5f7fe549c14764d6c6488ac00000000";
            var tx = new Transaction(data);

            var id = tx.GetHash();
            Console.WriteLine($"ID: {id}");
        }
    }
}
