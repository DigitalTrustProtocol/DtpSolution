using DtpCore.Model;
using DtpPackageCore.Model;
using Ipfs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageService
    {
        Task<Peer> GetLocalPeer();
        Task<IEnumerable<Peer>> GetPeersAsync(string scope);
        Task AddPackageSubscriptionsAsync();
        Task AddPackageSubscriptionsAsync(string scope);
        void PublishPackageMessageAsync(PackageMessage packageMessage);
        Task<Package> FetchPackageAsync(string path);
        Task<string> StorePackageAsync(Package package);

    }
}