using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DtpCore.Interfaces
{
    public interface ITrustDBService
    {
        
        IQueryable<Package> Packages { get; }
        IQueryable<Claim> Claims { get; }
        IQueryable<Timestamp> Timestamps { get; }
        IQueryable<WorkflowContainer> Workflows { get; }

        TrustDBContext DBContext { get; }
        long ID { get; set; }

        bool DoClaimExist(byte[] id);
        Claim GetClaimById(byte[] id);
        IQueryable<Claim> GetClaims(string issuerId, string subjectId, string scopeValue);
        IQueryable<Claim> GetActiveClaims(ClaimStateType exclude = ClaimStateType.Replaced);
        Claim GetSimilarClaim(Claim trust);

        void Add(Claim claim);
        void Remove(Claim claim);
        void Add(Package package);
        void Remove(Package package);

        //void Remove(ClaimPackageRelationship claimPackageRelationship);

        //bool Add(Package package);
        void Update(Claim claim);
        void Update(Package package);

        Task<bool> DoPackageExistAsync(byte[] packageId);
        Package GetPackageById(byte[] packageId);
        Task<List<Package>> GetBuildPackages();
        Package GetBuildPackage(string scope);
        Package EnsureBuildPackage(string scope);
        void LoadPackageClaims(Package package);

        void EnsurePackageState(Package package);

        void SaveChanges();
    }
}