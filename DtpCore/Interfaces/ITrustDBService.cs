using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;
using DtpCore.Model.Database;
using DtpCore.Repository;

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

        bool DoTrustExist(byte[] id);
        Claim GetClaimById(byte[] id);
        IQueryable<Claim> GetClaims(string issuerId, string subjectId, string scopeValue);
        IQueryable<Claim> GetActiveClaims(ClaimState exclude = ClaimState.Replaced);
        Claim GetSimilarClaim(Claim trust, ClaimState exclude = ClaimState.Replaced);

        void Add(Claim claim);
        //bool Add(Package package);
        void Update(Claim claim);
        
        Package GetPackage(byte[] packageId);

        
    }
}