using DtpCore.Model;
using DtpCore.ViewModel;
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
        void AddPackageSubscriptions();
        void AddPackageSubscriptions(string scope);
        void PublishPackageMessageAsync(PackageMessage packageMessage);
        Task<PackageInfoCollection> GetPackageInfoCollectionAsync(string ipAddress, string scope, long from);
        Task<Package> FetchPackageAsync(string path);
        Task<string> StorePackageAsync(Package package);

    }
}