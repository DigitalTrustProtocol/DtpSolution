using DtpCore.Model;
using DtpPackageCore.Model;
using System.Threading.Tasks;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageService
    {
        Task AddPackageSubscriptionsAsync();
        Task AddPackageSubscriptionsAsync(string scope);
        void PublishPackageMessageAsync(PackageMessage packageMessage);
        Task<Package> FetchPackageAsync(string path);
        Task<string> StorePackageAsync(Package package);

    }
}