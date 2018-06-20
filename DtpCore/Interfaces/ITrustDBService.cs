using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;
using DtpCore.Repository;

namespace DtpCore.Interfaces
{
    public interface ITrustDBService
    {
        
        IQueryable<Package> Packages { get; }
        IQueryable<Trust> Trusts { get; }
        IQueryable<Timestamp> Timestamps { get; }
        IQueryable<WorkflowContainer> Workflows { get; }

        TrustDBContext DBContext { get; }
        long ID { get; set; }

        bool TrustExist(byte[] id);
        Trust GetTrustById(byte[] id);
        IQueryable<Trust> GetTrusts(byte[] issuerAddress, byte[] subjectAddress, string scopeValue);
        Trust GetSimilarTrust(Trust trust);

        void Add(Trust trust);
        bool Add(Package package);
        void Update(Trust trust);
        
        Package GetPackage(byte[] packageId);

        
    }
}