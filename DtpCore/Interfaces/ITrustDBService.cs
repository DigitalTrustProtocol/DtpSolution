using System.Collections.Generic;
using System.Linq;
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
        Claim GetSimilarClaim(Claim trust, ClaimStateType exclude = ClaimStateType.Replaced);

        void Add(Claim claim);
        void Add(Package package);

        //bool Add(Package package);
        void Update(Claim claim);
        void Update(Package package);

        Package GetPackageById(byte[] packageId);
        Package GetBuildPackage(Package package, Claim claim);

        void EnsurePackageState(Package package);

        void SaveChanges();
    }
}