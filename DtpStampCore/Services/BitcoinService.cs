using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using DtpStampCore.Interfaces;
using DtpStampCore.Extensions;
using Newtonsoft.Json.Linq;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpStampCore.Model;

namespace DtpStampCore.Services
{
    public class BitcoinService : IBlockchainService
    {
        public Network Network { get; set; }
        public IDerivationStrategy DerivationStrategy { get; set; }

        public IBlockchainRepository Repository { get; set; }

        private IDerivationStrategyFactory _derivationStrategyFactory;

        public BitcoinService(IBlockchainRepository repository, IDerivationStrategyFactory derivationStrategyFactory)
        {
            Repository = repository;
            _derivationStrategyFactory = derivationStrategyFactory;

            DerivationStrategy = _derivationStrategyFactory.GetService("btc-pkh");
            Network = Network.Main;
            DerivationStrategy.NetworkName = Network.NetworkType.ToString();
        }

        /// <summary>
        /// Verify that there are sufficient funds available on the key.
        /// </summary>
        /// <param name="fundingKey">The private key provided to the timestamp service.</param>
        /// <param name="previousTx">Previous used transactions, make it possible to spend unconfirmed transactions</param>
        /// <returns>0 = sufficient funds. 1 = No coins to spend, 2 = Not enough coin to spend</returns>
        public int VerifyFunds(byte[] fundingKey, IList<byte[]> previousTx = null)
        {
            //var serverAddress2 = DerivationStrategy.GetAddress(fundingKey);
            var serverKey = new Key(fundingKey);
            var serverAddress = serverKey.PubKey.GetAddress(Network);
            
            var fee = Repository.GetEstimatedFee().FeePerK;

            var coins = GetCoins(previousTx, fee, serverAddress);

            var result = EnsureFee(fee, coins);

            return result;
        }

        /// <summary>
        /// Checks the address for received transactions and returns highest number of confimations from all transactions.
        /// </summary>
        /// <param name="merkleRoot">The private key of the source hash</param>
        /// <returns>-1 = no Timestamps, 0 = unconfirmed tx, above 0 is the number of confimations</returns>
        public AddressTimestamp GetTimestamp(byte[] merkleRoot)
        {
            var result = new AddressTimestamp();
            var key = new Key(DerivationStrategy.GetKey(merkleRoot));
            var address = key.PubKey.GetAddress(Network);
            result.Address = address.Hash.ToBytes(); // Address without Network format

            var json = Repository.GetReceivedAsync(address.ToString()).Result; //.ToWif());

            var txs = json["data"]["txs"];
            if (txs == null)
                return result; // -1

            if (txs.Count() == 0)
                return result; // -1

            result.Time = txs.Min(p => p["time"].ToInteger());
            result.Confirmations = txs.Max(p => p["confirmations"].ToInteger());

            return result;
        }

        /// <summary>
        /// Submits transactions on the hash address.
        /// </summary>
        /// <param name="merkleRoot">The hash of the merkle root node</param>
        /// <param name="fundingKey">The server private key used for funding</param>
        /// <param name="previousTx">Output transaction from the last timestamp</param>
        /// <returns>The output transactions, can be used as input transaction for the next timestamp before confimation</returns>
        public IList<byte[]> Send(byte[] merkleRoot, byte[] fundingKey, IList<byte[]> previousTx = null)
        {
            var serverKey = new Key(fundingKey);
            var serverAddress = serverKey.PubKey.GetAddress(Network);
            var txs = new List<byte[]>();

            Key merkleRootKey = new Key(DerivationStrategy.GetKey(merkleRoot));
            
            var fee = Repository.GetEstimatedFee().FeePerK;

            var coins = GetCoins(previousTx, fee, serverAddress);
            if(EnsureFee(fee, coins) > 0)
                throw new ApplicationException("Not enough coin to spend.");

            var sourceTx = new TransactionBuilder()
                .AddCoins(coins)
                .AddKeys(serverKey)
                .Send(merkleRootKey.PubKey.GetAddress(Network), fee) // Send to Batch address
                .SendFees(fee)
                .SetChange(serverAddress)
                .BuildTransaction(true);

            Repository.BroadcastAsync(sourceTx);
            txs.Add(sourceTx.ToBytes());

            var txNota = new TransactionBuilder()
                .AddCoins(sourceTx.Outputs.AsCoins())
                .SendOP_Return(merkleRoot) // Put batch root on the OP_Return out tx
                .AddKeys(merkleRootKey)
                .SendFees(fee)
                .SetChange(serverAddress)
                .BuildTransaction(true);

            Repository.BroadcastAsync(txNota);
            txs.Add(txNota.ToBytes());

            return txs;
        }

        private IEnumerable<Coin> GetCoins(IList<byte[]> previousTx, Money fee, BitcoinAddress address)
        {
            IEnumerable<Coin> coins = null;
            long sumOfCoins = 0;
            if (previousTx != null)
            {
                foreach (var rawTx in previousTx)
                {
                    var tx = new Transaction(rawTx);
                    coins = tx.Outputs.AsCoins().Where(c => c.ScriptPubKey.GetDestinationAddress(Network) == address);
                    sumOfCoins = coins.Sum(c => c.Amount.Satoshi);
                }
            }

            if (fee.Satoshi * 2 > sumOfCoins)
            {
                var unspent = Repository.GetUnspentAsync(address.ToString()); //.ToWif());
                unspent.Wait();
                var obj = unspent.Result;

                coins = ParseTX(obj);
            }

            return coins;
        }

        private List<Coin> ParseTX(JObject json)
        {
            List<Coin> list = new List<Coin>();
            foreach (var element in json["data"]["txs"])
            {
                list.Add(new Coin(uint256.Parse(element["txid"].ToString()), (uint)element["output_no"], new Money((decimal)element["value"], MoneyUnit.BTC), new Script(NBitcoin.DataEncoders.Encoders.Hex.DecodeData(element["script_hex"].ToString()))));
            }
            return list;
        }

        private int EnsureFee(Money fee, IEnumerable<Coin> coins)
        {
            if (coins.Count() == 0)
                return 1;

            var sumOfCoins = coins.Sum(c => c.Amount.Satoshi);
            if (fee.Satoshi * 2 > sumOfCoins)
                return 2;
            return 0;
        }
    }
}
