using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using DtpCore.Extensions;
using DtpCore.Model.Database;
using DtpCore.Builders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DtpCore.Services
{

    public class TrustDBService : ITrustDBService
    {
        private static object lockObj = new object();

        public long ID { get; set; }
        public TrustDBContext DB { get; }

        public IQueryable<Package> Packages
        {
            get
            {
                return DB.Packages
                .Include(c => c.Timestamps);
            }
        }

        public IQueryable<Claim> Claims
        {
            get
            {
                return DB.Claims;
            }
        }


        public IQueryable<Timestamp> Timestamps
        {
            get
            {
                return DB.Timestamps.AsQueryable();
            }
        }

        public IQueryable<WorkflowContainer> Workflows
        {
            get
            {
                return DB.Workflows.AsQueryable();
            }
        }

        public TrustDBService(TrustDBContext trustDBContext)
        {
            DB = trustDBContext;
        }

        public bool DoClaimExist(byte[] id)
        {
            if (id == null || id.Length == 0)
                return false;

            var dbTrust = DB.Claims.FirstOrDefault(p => p.Id == id);
            
            return (dbTrust != null);
        }

        public Claim GetClaimById(byte[] id)
        {
            var dbTrust = DB.Claims
                .Include(cl => cl.Timestamps)
                .Include(p => p.ClaimPackages)
                .FirstOrDefault(p => p.Id == id);
            return dbTrust;
        }


        public IQueryable<Claim> GetClaims(IQueryable<Claim> query, string issuerId, string subjectId, string scope, string type)
        {
            
            if (!string.IsNullOrEmpty(issuerId))
                query = query.Where(p => p.Issuer.Id.Equals(issuerId));

            if (!string.IsNullOrEmpty(subjectId))
                query = query.Where(p => p.Subject.Id.Equals(subjectId));


            if (!string.IsNullOrEmpty(scope))
            {
                scope = scope.ToLowerInvariant();
                query = query.Where(p => p.Scope == scope);
            }

            if (!string.IsNullOrEmpty(type))
            {
                type = type.ToLowerInvariant();
                query = query.Where(p => p.Type == type);
            }
            
            return query;
        }

        public IQueryable<Claim> GetActiveClaims(IQueryable<Claim> query, ClaimStateType exclude = ClaimStateType.Replaced)
        {
            var time = DateTime.Now.ToUnixTime();

            return query.Where(p=> (p.Activate <= time || p.Activate == 0)
                         && (p.Expire > time || p.Expire == 0)
                         && (p.State & exclude) == 0);
        }


        public IQueryable<Claim> AddClaimMeta(IQueryable<Claim> query)
        {
            var result = from c in query
                         join issuer in DB.IdentityMetadata on c.Issuer.Id + c.Scope equals issuer.Id into issuerMeta
                         from issuerItem in issuerMeta.DefaultIfEmpty()
                         join subject in DB.IdentityMetadata on c.Subject.Id + c.Scope equals subject.Id into subjectMeta
                         from subjectItem in subjectMeta.DefaultIfEmpty()
                         select (c != null) ? c.UpdateMeta(issuerItem, subjectItem) : null;

            return result;
        }


        /// <summary>
        /// Get the same claim or a similar claim from database.
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public Claim GetSimilarClaim(Claim claim)
        {
            var query = from p in DB.Claims select p;

            query = query.Where(p => p.Id == claim.Id 
                            || (p.Issuer.Id == claim.Issuer.Id
                              && p.Subject.Id == claim.Subject.Id
                              && p.Type == claim.Type
                              && p.State != ClaimStateType.Replaced));

            if (claim.Scope != null)
            {
                query = query.Where(p => p.Scope == claim.Scope);
            }
            else
            {
                query = query.Where(p => p.Scope == null);
            }

            query = query.OrderBy(p => p.DatabaseID);

            var dbClaim = query.Take(1).FirstOrDefault();

            return dbClaim;
        }


        public IQueryable<Claim> BindTitle(IQueryable<Claim> query)
        {
            
            return query;
        }


        public void Add(Claim claim)
        {
            DB.Claims.Add(claim);
        }

        public void Remove(Claim claim)
        {
            DB.Remove(claim);
        }

        public void Add(Package package)
        {
            DB.Packages.Add(package);
        }

        public void Remove(Package package)
        {
            DB.Remove(package);
        }



        public async Task<List<Package>> GetBuildPackagesAsync()
        {
            // Check if there is a builder package ready
            var buildPackages = DB.Packages
                .Where(p => (p.State & PackageStateType.Building) > 0)
                .Include(p => p.ClaimPackages)
                .ThenInclude(p => p.Claim);

            return await buildPackages.ToListAsync();
        }


        /// <summary>
        /// Gets the build package where all new claims are added.
        /// </summary>
        /// <returns></returns>
        public Package GetBuildPackage(string scope)
        {
            lock (lockObj)
            {
                return EnsureBuildPackage(scope);
            }
        }

        public Package EnsureBuildPackage(string scope)
        {
            // Check if there is a builder package ready
            var buildPackage = DB.Packages
                .Where(p => (p.State & PackageStateType.Building) > 0 && p.Scopes == scope)
                .Include(p=>p.ClaimPackages)
                .ThenInclude(p=>p.Claim)
                .FirstOrDefault();

            if (buildPackage == null)
            {
                // Create a new builder package, make sure that this is done syncronius.
                buildPackage = (new PackageBuilder()).Package;
                buildPackage.State = PackageStateType.Building;
                buildPackage.Scopes = scope;

                Add(buildPackage);
                DB.SaveChanges();
            }
            return buildPackage;
        }

        public void LoadPackageClaims(Package package)
        {
            if (package == null)
                return;

            package.ClaimPackages = DB.ClaimPackageRelationships.Where(p => p.PackageID == package.DatabaseID).Include(p => p.Claim).ToList();
            package.Claims = package.ClaimPackages.OrderBy(p => p.Claim.DatabaseID).Select(p => p.Claim).ToList();
        }


        public void Update(Claim trust)
        {
            DB.Claims.Update(trust);
        }

        public void Update(Package package)
        {
            DB.Packages.Update(package);
        }


        public async Task<bool> DoPackageExistAsync(byte[] packageId)
        {
            if (packageId == null || packageId.Length == 0)
                return false;

            return await Packages.AsNoTracking().AnyAsync(f => f.Id == packageId); 
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

            if(package.Server != null && package.Server.Proof != null && package.Server.Proof.Length > 0)
                package.State = PackageStateType.Signed;
        }

        public void SaveChanges()
        {
            DB.SaveChanges();
        }
    }
}
