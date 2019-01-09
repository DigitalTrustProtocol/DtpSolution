using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;
using DtpCore.Repository;

namespace DtpCore.Interfaces
{
    public interface ITrustDBService
    {
        
        IQueryable<Package> Packages { get; }
        IQueryable<Claim> Trusts { get; }
        IQueryable<Timestamp> Timestamps { get; }
        IQueryable<WorkflowContainer> Workflows { get; }

        TrustDBContext DBContext { get; }
        long ID { get; set; }

        bool TrustExist(byte[] id);
        Claim GetClaimById(byte[] id);
        IQueryable<Claim> GetTrusts(string issuerId, string subjectId, string scopeValue);
        IQueryable<Claim> GetActiveTrust();
        Claim GetSimilarClaim(Claim trust);

        void Add(Claim trust);
        //bool Add(Package package);
        void Update(Claim trust);
        
        Package GetPackage(byte[] packageId);

        
    }
}