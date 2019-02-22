using DtpCore.Model;
using DtpCore.ViewModel;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using Ipfs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.DtpPackage;
using UnitTest.TestData;

namespace UnitTest.DtpPackageCore.Mocks
{
    public class PackageServiceMock : IPackageService
    {
        private PackageMessage tempMessage;
        private Peer self = new Peer
        {
            AgentVersion = "self",
            Id = "QmXK9VBxaXFuuT29AaPUTgW3jBWZ9JgLVZYdMYTHC6LLAH",
            PublicKey = "CAASXjBcMA0GCSqGSIb3DQEBAQUAA0sAMEgCQQCC5r4nQBtnd9qgjnG8fBN5+gnqIeWEIcUFUdCG4su/vrbQ1py8XGKNUBuDjkyTv25Gd3hlrtNJV3eOKZVSL8ePAgMBAAE="

        };
        private Peer other = new Peer
        {
            AgentVersion = "other",
            Id = "QmdpwjdB94eNm2Lcvp9JqoCxswo3AKQqjLuNZyLixmCM1h",
            PublicKey = "CAASXjBcMA0GCSqGSIb3DQEBAQUAA0sAMEgCQQDlTSgVLprWaXfmxDr92DJE1FP0wOexhulPqXSTsNh5ot6j+UiuMgwb0shSPKzLx9AuTolCGhnwpTBYHVhFoBErAgMBAAE=",
            Addresses = new MultiAddress[] { "/ip4/192.168.0.82/tcp/4001/ipfs/QmdpwjdB94eNm2Lcvp9JqoCxswo3AKQqjLuNZyLixmCM1h" },
            ConnectedAddress = new MultiAddress("/ip4/192.168.0.82/tcp/4001/ipfs/QmdpwjdB94eNm2Lcvp9JqoCxswo3AKQqjLuNZyLixmCM1h")
        };

        public void AddPackageSubscriptions()
        {
        }

        public void AddPackageSubscriptions(string scope)
        {
        }

        public Task<Package> FetchPackageAsync(string path)
        {
            var package = TestPackage.CreateBinary(1);
            return Task.FromResult(package);
        }

        public Task<Peer> GetLocalPeer()
        {
            return IPFSTest.Ipfs.LocalPeer.Task;
        }

        public Task<PackageInfoCollection> GetPackageInfoCollectionAsync(string ipAddress, string scope, long from)
        {
            var info = new PackageInfoCollection();
            info.Packages.Add(new PackageInfo { Id = Encoding.UTF8.GetBytes("123"), File = "123" });
            return Task.FromResult(info);
        }

        public Task<IEnumerable<Peer>> GetPeersAsync(string scope)
        {
            var list = new List<Peer>();

            list.Add(GetLocalPeer().GetAwaiter().GetResult());
            list.Add(other);

            return Task.FromResult(list.AsEnumerable());
        }

        public void PublishPackageMessageAsync(PackageMessage packageMessage)
        {
            tempMessage = packageMessage;
        }

        public Task<string> StorePackageAsync(Package package)
        {
            return Task.FromResult("123");
        }
    }
}
