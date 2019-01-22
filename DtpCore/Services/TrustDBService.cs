using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections;
using DtpCore.Extensions;
using DtpCore.Model.Database;

namespace DtpCore.Services
{
    public class TrustDBService : ITrustDBService
    {
        public long ID { get; set; }
        public TrustDBContext DBContext { get; }

        public IQueryable<Package> Packages
        {
            get
            {
                return DBContext.Packages
                .Include(c => c.Timestamps)
                .Include(c => c.Claims);
            }
        }

        public IQueryable<Claim> Claims
        {
            get
            {
                return DBContext.Claims;
            }
        }


        public IQueryable<Timestamp> Timestamps
        {
            get
            {
                return DBContext.Timestamps.AsQueryable();
            }
        }

        public IQueryable<WorkflowContainer> Workflows
        {
            get
            {
                return DBContext.Workflows.AsQueryable();
            }
        }

        public TrustDBService(TrustDBContext trustDBContext)
        {
            DBContext = trustDBContext;
        }

        public bool DoClaimExist(byte[] id)
        {
            // GetClaimById is slower because of includes of relative tables.
            var dbTrust = DBContext.Claims.FirstOrDefault(p => p.Id == id);
            //var dbTrust = GetClaimById(id);
            return (dbTrust != null);
        }

        public Claim GetClaimById(byte[] id)
        {
            var dbTrust = DBContext.Claims
                .Include(p => p.Timestamps)
                .Include(p => p.ClaimPackages)
                .FirstOrDefault(p => p.Id == id);
            return dbTrust;
        }


        public IQueryable<Claim> GetClaims(string issuerId, string subjectId, string scopeValue)
        {
            var query = from p in DBContext.Claims
                        where p.Issuer.Id == issuerId
                              && p.Subject.Id == subjectId
                        select p;

            if (scopeValue != null)
                query = query.Where(p => p.Scope == scopeValue);

            return query;
        }

        public IQueryable<Claim> GetActiveClaims(ClaimState exclude = ClaimState.Replaced)
        {
            var time = DateTime.Now.ToUnixTime();

            var trusts = from trust in DBContext.Claims
                         where (trust.Activate <= time || trust.Activate == 0) 
                         && (trust.Expire > time || trust.Expire == 0) 
                         && (trust.State & exclude) == 0
                         select trust;

            return trusts;
        }

        public Claim GetSimilarClaim(Claim trust, ClaimState exclude = ClaimState.Replaced)
        {
            var query = from p in DBContext.Claims select p;

            query = query.Where(p => p.Issuer.Id == trust.Issuer.Id
                              && p.Subject.Id == trust.Subject.Id
                              && p.Type == trust.Type
                              && (p.State & exclude) == 0);


            if (trust.Scope != null)
            {
                query = query.Where(p => p.Scope == trust.Scope);
            }
            else
            {
                query = query.Where(p => p.Scope == null);
            }

            var dbTrust = query.FirstOrDefault();

            return dbTrust;
        }


        public void Add(Claim trust)
        {
            DBContext.Claims.Add(trust);
        }

        //public bool Add(Package package)
        //{
        //    //if(package.Id == null || package.Id.Length == 0)
        //    //{
        //    //    //var builder = new TrustBuilder()
        //    //}

        //    if (DBContext.Packages.Any(f => f.Id == package.Id))
        //        throw new ApplicationException("Package already exist");

        //    foreach (var trust in package.Trusts.ToArray())
        //    {
        //        var dbTrust = DBContext.Trusts.FirstOrDefault(p => p.Id == trust.Id);
        //        if (dbTrust == null)
        //            continue;


        //        //if (package.Timestamps == null && trust.Timestamp == null)
        //        //{
        //        //    package.Trusts.Remove(trust);
        //        //    continue;
        //        //}

        //        //if (dbTrust.Timestamp == null)
        //        //{
        //        //    DBContext.Trusts.Remove(dbTrust);
        //        //    continue;
        //        //}

        //        // Check timestamp
        //    }

        //    DBContext.Packages.Add(package);
        //    DBContext.SaveChanges();
        //    return true;
        //}

        public void Update(Claim trust)
        {
            DBContext.Claims.Update(trust);
        }

        public Package GetPackage(byte[] packageId)
        {
            var task = Packages.SingleOrDefaultAsync(f => f.Id == packageId); 

            task.Wait();

            return task.Result;
        }

    }
}
