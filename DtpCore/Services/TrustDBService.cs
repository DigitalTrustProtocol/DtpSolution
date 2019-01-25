using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections;
using DtpCore.Extensions;
using DtpCore.Model.Database;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using DtpCore.Builders;

namespace DtpCore.Services
{
    public class TrustDBService : ITrustDBService
    {
        private static object lockObj = new object();

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

        public IQueryable<Claim> GetActiveClaims(ClaimStateType exclude = ClaimStateType.Replaced)
        {
            var time = DateTime.Now.ToUnixTime();

            var trusts = from trust in DBContext.Claims
                         where (trust.Activate <= time || trust.Activate == 0) 
                         && (trust.Expire > time || trust.Expire == 0) 
                         && (trust.State & exclude) == 0
                         select trust;

            return trusts;
        }

        public Claim GetSimilarClaim(Claim trust, ClaimStateType exclude = ClaimStateType.Replaced)
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

        public void Add(Package package)
        {
            DBContext.Packages.Add(package);
        }

        [ThreadStatic]
        private Package buildPackage = null;

        public Package GetBuildPackage(Package package, Claim claim)
        {
            if (package != null)
            {
                if (package.State.Match(PackageStateType.Signed | PackageStateType.Building))
                    return package;
            }

            lock (lockObj)
            {
                //if (buildPackage != null)
                //    return buildPackage;

                // Check if there is a builder package ready
                buildPackage = DBContext.Packages.Where(p => (p.State & PackageStateType.Building) > 0).OrderBy(p => p.Created).FirstOrDefault();
                if (buildPackage == null)
                {

                    // Create a new builder package, make sure that this is done syncronius.
                    if (package == null)
                        package = (new PackageBuilder()).Package;
                    else
                        buildPackage = package;

                    buildPackage.State = PackageStateType.Building;

                    if (buildPackage.DatabaseID == 0)
                        Add(buildPackage);
                    else
                        Update(buildPackage);
                }
                
                return buildPackage;
            }
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

        public void Update(Package package)
        {
            DBContext.Packages.Update(package);
        }

        public Package GetPackageById(byte[] packageId)
        {
            if (packageId == null || packageId.Length == 0)
                return null;

            var task = Packages.SingleOrDefaultAsync(f => f.Id == packageId); 

            return task.GetAwaiter().GetResult();
        }

        public void EnsurePackageState(Package package)
        {
            if (package == null)
                return;

            if (package.State > 0)
                return;

            package.State = PackageStateType.New;

            // Packages without ID is a client submitted packages containing new claims.
            // Packages with ID, is commonly a package from another server.
            if ((package.Id != null && package.Id.Length > 0))
                package.State = PackageStateType.Build;

            if(package.Server != null && package.Server.Signature != null && package.Server.Signature.Length > 0)
                package.State = PackageStateType.Signed;
        }

        public void SaveChanges()
        {
            DBContext.SaveChanges();
        }
    }
}
