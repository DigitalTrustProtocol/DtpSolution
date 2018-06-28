using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections;

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
                .Include(c => c.Trusts);
            }
        }

        public IQueryable<Trust> Trusts
        {
            get
            {
                return DBContext.Trusts;
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

        public bool TrustExist(byte[] id)
        {
            var dbTrust = GetTrustById(id);
            return (dbTrust != null);
        }

        public Trust GetTrustById(byte[] id)
        {
            var dbTrust = DBContext.Trusts.AsNoTracking().Include(p => p.Timestamps).FirstOrDefault(p => StructuralComparisons.StructuralEqualityComparer.Equals(p.Id, id));
            return dbTrust;
        }


        public IQueryable<Trust> GetTrusts(string issuerAddress, string subjectAddress, string scopeValue)
        {
            var query = from p in DBContext.Trusts
                        where p.Issuer.Address == issuerAddress
                              && p.Subject.Address == subjectAddress
                        select p;

            if (scopeValue != null)
                query = query.Where(p => p.Scope.Value == scopeValue);

            return query;
        }


        public Trust GetSimilarTrust(Trust trust)
        {
            var query = from p in DBContext.Trusts select p;

            query = query.Where(p => p.Issuer.Address == trust.Issuer.Address
                              && p.Subject.Address == trust.Subject.Address
                              && p.Type == trust.Type
                              && p.Replaced == false);

            if (trust.Scope != null)
            {
                query = query.Where(p => p.Scope.Value == trust.Scope.Value);
            }
            else
            {
                query = query.Where(p => p.Scope.Value == null);
            }

            var dbTrust = query.FirstOrDefault();

            return dbTrust;
        }

        public void Add(Trust trust)
        {
            DBContext.Trusts.Add(trust);
        }

        public bool Add(Package package)
        {
            //if(package.Id == null || package.Id.Length == 0)
            //{
            //    //var builder = new TrustBuilder()
            //}

            if (DBContext.Packages.Any(f => f.Id == package.Id))
                throw new ApplicationException("Package already exist");

            foreach (var trust in package.Trusts.ToArray())
            {
                var dbTrust = DBContext.Trusts.FirstOrDefault(p => StructuralComparisons.StructuralEqualityComparer.Equals(p.Id, trust.Id));
                if (dbTrust == null)
                    continue;


                //if (package.Timestamps == null && trust.Timestamp == null)
                //{
                //    package.Trusts.Remove(trust);
                //    continue;
                //}

                //if (dbTrust.Timestamp == null)
                //{
                //    DBContext.Trusts.Remove(dbTrust);
                //    continue;
                //}

                // Check timestamp
            }

            DBContext.Packages.Add(package);
            DBContext.SaveChanges();
            return true;
        }

        public void Update(Trust trust)
        {
            DBContext.Trusts.Update(trust);
        }

        public Package GetPackage(byte[] packageId)
        {
            var task = Packages.SingleOrDefaultAsync(f => f.Id == packageId); 

            task.Wait();

            return task.Result;
        }

    }
}
