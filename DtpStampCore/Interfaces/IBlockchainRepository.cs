using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json.Linq;

namespace DtpStampCore.Interfaces
{
    public interface IBlockchainRepository
    {
        Task BroadcastAsync(Transaction tx);
        Task<JObject> GetReceivedAsync(string address);
        Task<JObject> GetUnspentAsync(string Address);
        FeeRate GetEstimatedFee();
        string ServiceUrl { get; }
        string AddressLookupUrl(string blockchain, string address);
    }
}