using NBitcoin;
using DtpStampCore.Interfaces;
using Microsoft.Extensions.Configuration;
using DtpCore.Interfaces;

namespace DtpStampCore.Services
{
    public class BitcoinTestService : BitcoinService
    {

        public BitcoinTestService(IBlockchainRepository blockchain, IDerivationStrategyFactory derivationStrategyFactory) : base(blockchain, derivationStrategyFactory)
        {
            Network = Network.TestNet;
            DerivationStrategy.NetworkName = Network.NetworkType.ToString();
        }
    }
}