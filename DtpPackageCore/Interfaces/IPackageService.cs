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
        Task AddPackageSubscriptionsAsync();
        Task AddPackageSubscriptionsAsync(string scope);
        void PublishPackageMessageAsync(PackageMessage packageMessage);
        PackageInfoCollection GetPackageInfoCollection(string ipAddress, string scope, long from);
        Task<Package> FetchPackageAsync(string path);
        Task<string> StorePackageAsync(Package package);

    }
}