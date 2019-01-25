using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using DtpCore.Extensions;
using DtpCore.Model.Database;
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
                .Include(c => c.Timestamps);
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
            if (id == null || id.Length == 0)
                return false;

            var dbTrust = DBContext.Claims.FirstOrDefault(p => p.Id == id);
            
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

        public Claim GetSimilarClaim(Claim trust)
        {
            var query = from p in DBContext.Claims select p;

            // No need
            //query  = query.Include(p => p.ClaimPackages).ThenInclude(p => p.Package);

            query = query.Where(p => p.Issuer.Id == trust.Issuer.Id
                              && p.Subject.Id == trust.Subject.Id
                              && p.Type == trust.Type);


            if (trust.Scope != null)
            {
                query = query.Where(p => p.Scope == trust.Scope);
            }
            else
            {
                query = query.Where(p => p.Scope == null);
            }

            query = query.OrderBy(p => p.DatabaseID);

            var dbTrust = query.Take(1).FirstOrDefault();

            return dbTrust;
        }


        public void Add(Claim trust)
        {
            DBContext.Claims.Add(trust);
        }

        public void Remove(Claim claim)
        {
            DBContext.Remove(claim);
        }

        public void Add(Package package)
        {
            DBContext.Packages.Add(package);
        }

        public void Remove(Package package)
        {
            DBContext.Remove(package);
        }


        /// <summary>
        /// There will always only be one build package.
        /// If package provided is build and signed, then it will be used.
        /// </summary>
        /// <returns></returns>
        public Package GetBuildPackage()
        {
            lock (lockObj)
            {
                return EnsureBuildPackage();
            }
        }

        public Package EnsureBuildPackage()
        {
            // Check if there is a builder package ready
            var buildPackage = DBContext.Packages.Where(p => (p.State & PackageStateType.Building) > 0).Include(p=>p.ClaimPackages).ThenInclude(p=>p.Claim).FirstOrDefault();
            if (buildPackage == null)
            {
                // Create a new builder package, make sure that this is done syncronius.
                buildPackage = (new PackageBuilder()).Package;
                buildPackage.State = PackageStateType.Building;

                Add(buildPackage);
                DBContext.SaveChanges();
            }
            return buildPackage;
        }

        public void LoadPackageClaims(Package package)
        {
            if (package == null)
                return;

            package.ClaimPackages = DBContext.ClaimPackageRelationships.Where(p => p.PackageID == package.DatabaseID).Include(p => p.Claim).ToList();
            package.Claims = package.ClaimPackages.OrderBy(p => p.Claim.DatabaseID).Select(p => p.Claim).ToList();
        }


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
