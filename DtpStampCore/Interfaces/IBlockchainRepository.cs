using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json.Linq;
using QBitNinja.Client.Models;

namespace DtpStampCore.Interfaces
{
    public interface IBlockchainRepository
    {
        Task BroadcastAsync(Transaction tx);
        Task<BalanceModel> GetReceivedAsync(string address);
        Task<BalanceModel> GetUnspentAsync(string Address);
        FeeRate GetEstimatedFee();
        string ServiceUrl { get; }
        string AddressLookupUrl(string blockchain, string address);
    }
}