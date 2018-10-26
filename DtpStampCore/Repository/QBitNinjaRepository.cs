using Microsoft.Extensions.Configuration;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DtpStampCore.Interfaces;
using QBitNinja.Client;
using DtpStampCore.Extensions;
using QBitNinja.Client.Models;
using NBitcoin.Protocol;

namespace DtpStampCore.Repository
{
    /// <summary>
    /// Code based on https://github.com/MetacoSA/QBitNinja/
    /// </summary>
    public class QBitNinjaException : Exception
    {
        internal QBitNinjaException(RejectCode code, string responseData)
            : base(code.ToString() + " : "+ responseData)
        {
            ResponseData = responseData;
            Code = code;
        }

        public string ResponseData
        {
            get;
            set;
        }
        public RejectCode Code
        {
            get;
            set;
        }
    }
    public class QBitNinjaRepository : IBlockchainRepository
    {
        public string ApiVersion = "v2";
        public string BlockchainName { get; set; }
        public Network BTCNetwork { get; set; }

        public const string MainUrl = "http://api.qbit.ninja/";
        public const string TestUrl = "http://tapi.qbit.ninja/";

        public QBitNinjaRepository(IConfiguration configuration)
        {
            BTCNetwork = Network.Main;
            var chain = configuration.Blockchain();
            switch(chain.ToLower())
            {
                case "btc": BTCNetwork = Network.Main; break;
                case "btctest": BTCNetwork = Network.TestNet; break;
                case "btcreg": BTCNetwork = Network.RegTest; break;
            }
            Client = new QBitNinjaClient(BTCNetwork);
        }


        public QBitNinjaClient Client;

        #region ITransactionRepository Members

        public string ServiceUrl
        {
            get
            {
                if(BTCNetwork == Network.Main)
                    return MainUrl;

                return TestUrl;
            }
        }

        private string GetBlockchainUrl(string blockchain)
        {
            switch (blockchain.ToLower())
            {
                case "btc": return MainUrl; 
                case "btctest": return TestUrl; 
            }
            return MainUrl;
        }
        //public string ApiUrl
        //{
        //    get
        //    {
        //        return $"{ServiceUrl}api/{ApiVersion}";
        //    }
        //}

        public string AddressLookupUrl(string blockchain, string address)
        {
            var url = GetBlockchainUrl(blockchain);
            return $"{url}balances/{address}";
        }

        public async Task<BalanceModel> GetUnspentAsync(string address)
        {

            if (address == null)
                throw new ArgumentNullException("address");

            var dest = new BalanceSelector(address);

            var response = await Client.GetBalance(dest, true);
            
            return response;
        }

        public async Task<BalanceModel> GetReceivedAsync(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var dest = new BalanceSelector(address);

            var response = await Client.GetBalance(dest, false);

            return response;
        }

        public async Task BroadcastAsync(Transaction tx)
        {
            if (tx == null)
                throw new ArgumentNullException("tx");

            BroadcastResponse broadcastResponse = await Client.Broadcast(tx);

            if (!broadcastResponse.Success)
            {
                throw new QBitNinjaException(broadcastResponse.Error.ErrorCode, broadcastResponse.Error.Reason);
            }
        }

        public FeeRate GetEstimatedFee()
        {
            return new FeeRate("0.0001");
        }


        #endregion
    }
}
