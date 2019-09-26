using Ipfs.CoreApi;
using System;

namespace UnitTest.DtpPackage.Mocks
{
    public class IpfsClientMock : ICoreApi
    {
        public IBitswapApi Bitswap => throw new NotImplementedException();

        public IBlockApi Block => throw new NotImplementedException();

        public IBootstrapApi Bootstrap => throw new NotImplementedException();

        public IConfigApi Config => throw new NotImplementedException();

        public IDagApi Dag => throw new NotImplementedException();

        public IDhtApi Dht => throw new NotImplementedException();

        public IDnsApi Dns => throw new NotImplementedException();

        public IFileSystemApi FileSystem { get; }

        public IGenericApi Generic => throw new NotImplementedException();

        public IKeyApi Key => throw new NotImplementedException();

        public INameApi Name => throw new NotImplementedException();

        public IObjectApi Object => throw new NotImplementedException();

        public IPinApi Pin => throw new NotImplementedException();

        public IPubSubApi PubSub { get; }

        public IStatsApi Stats => throw new NotImplementedException();

        public ISwarmApi Swarm => throw new NotImplementedException();

        public IBlockRepositoryApi BlockRepository => throw new NotImplementedException();

        public IpfsClientMock()
        {
            FileSystem = new FileSystemApiMock();
            PubSub = new PubSubApiMock();
        }
    }
}
