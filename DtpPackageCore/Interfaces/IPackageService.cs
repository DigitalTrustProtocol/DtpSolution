using DtpPackageCore.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DtpPackageCore.Interfaces
{
    public interface IPackageService
    {
        string CreatePackageName();

        void AddPackageSubscriptions();
        Task AddPackageSubscriptions(IEnumerable<string> claimTypes, IEnumerable<string> claimScopes);
        void PublishPackageMessage(PackageMessage packageMessage);
    }
}