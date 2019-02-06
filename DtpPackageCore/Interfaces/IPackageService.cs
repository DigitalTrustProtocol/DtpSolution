using DtpCore.Model;
using DtpPackageCore.Model;
using System.Threading.Tasks;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageService
    {
        void AddPackageSubscriptions();
        Task AddPackageSubscriptions(string scope);
        void PublishPackageMessage(PackageMessage packageMessage);
        Task<Package> FetchPackage(string path);
        Task<string> StorePackage(Package package);

    }
}