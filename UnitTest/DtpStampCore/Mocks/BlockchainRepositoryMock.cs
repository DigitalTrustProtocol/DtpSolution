using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json.Linq;
using DtpStampCore.Interfaces;
using Microsoft.Extensions.Configuration;
using QBitNinja.Client.Models;
using DtpCore.Extensions;

namespace UnitTest.DtpStampCore.Mocks
{
    public class BlockchainRepositoryMock : IBlockchainRepository
    {
        public string ApiVersion { get; set; } = "v2";
        public string BlockchainName { get; set; }

        public string ServiceUrl
        {
            get
            {
                return "https://test/";
            }
        }



        public static BalanceModel StandardData { get; set; } = new BalanceModel
        {
            Operations = new List<BalanceOperation>
            {
                new BalanceOperation
                {
                    Amount = 0L,
                    TransactionId = uint256.Parse("8e8bdc68a4546962bf21582af8c827cb6f27715986391bdbbeee8b2b19488896"),
                    FirstSeen = DatetimeExtensions.FromUnixTime(1509013624),
                    Confirmations= 2487,
                    ReceivedCoins = new List<ICoin>
                    {
                        new Coin
                        {
                            Amount = new Money(0.10771213m, MoneyUnit.BTC),
                            Outpoint = new OutPoint(),
                            ScriptPubKey = new Script("OP_DUP OP_HASH160 fe7f117cdd180643e8efbf5a60a151bf8afde947 OP_EQUALVERIFY OP_CHECKSIG"),
                        }

                    }
                }
            }
        };

        public static BalanceModel ReceivedData { get; set; } = StandardData;

        //= @"{
        //    ""data"" : {
        //        ""txs"" : [
        //                {
        //                    ""confirmations"" : 10
        //                }
        //            ]
        //        }
        //    }";

        public static BalanceModel UnspentData { get; set; } = StandardData;
            
            /*
            @"{
                ""data"" : {
                    ""txs"" : [
                            {
                                ""txid"" : ""8e8bdc68a4546962bf21582af8c827cb6f27715986391bdbbeee8b2b19488896"",
                                ""output_no"" : 1,
                                ""script_asm"" : ""OP_DUP OP_HASH160 fe7f117cdd180643e8efbf5a60a151bf8afde947 OP_EQUALVERIFY OP_CHECKSIG"",
                                ""script_hex"" : ""76a914fe7f117cdd180643e8efbf5a60a151bf8afde94788ac"",
                                ""value"" : ""0.10771213"",
                                ""confirmations"" : 2487,
                                ""time"" : 1509013624                            }
                        ]
                    }
                }";
                */
        public BlockchainRepositoryMock(IConfiguration configuration)
        {
            BlockchainName = (!String.IsNullOrWhiteSpace(configuration["blockchain"])) ? configuration["blockchain"] : "btctest";
        }

        public async Task BroadcastAsync(Transaction tx)
        {
            await Task.Run(() => true);
        }

        public FeeRate GetEstimatedFee()
        {
            return new FeeRate(new Money(100L));
        }

        public Task<BalanceModel> GetReceivedAsync(string address)
        {
            return Task.Run<BalanceModel>(() => ReceivedData);
        }

        public Task<BalanceModel> GetUnspentAsync(string Address)
        {
            return Task.Run<BalanceModel>(() => UnspentData);
        }

        public string AddressLookupUrl(string blockchain, string address)
        {
            return "None";
        }
    }
}
