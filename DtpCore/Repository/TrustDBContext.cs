using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace DtpCore.Repository
{
    public class TrustDBContext : DbContext
    {
        public DbSet<Package> Packages { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Timestamp> Timestamps { get; set; }
        public DbSet<BlockchainProof> Proofs { get; set; }
        public DbSet<ClaimPackageRelationship> ClaimPackageRelationships { get; set; }

        public DbSet<WorkflowContainer> Workflows { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }

        public TrustDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Package>().HasKey(p => p.DatabaseID);
            builder.Entity<Package>().HasIndex(p => p.Id);
            builder.Entity<Package>().OwnsOne(p => p.Server);
            builder.Entity<Package>()
                .HasMany(p => p.Claims)
                .WithOne()
                .HasForeignKey(c => c.PackageDatabaseID);

            //builder.Entity<Package>()
            //    .HasMany(p => p.Packages)
            //    .WithOne()
            //    .HasForeignKey(c => c.ParentID);

            //builder.Entity<Package>()
            //    .HasMany(p => p.Obsoletes)
            //    .WithOne()
            //    .HasForeignKey(c => c.ParentID);
            

            builder.Entity<Package>()
                .Property(e => e.Types)
                .HasConversion(v => string.Join('|', v),
                    v => v.Split('|', StringSplitOptions.RemoveEmptyEntries));

            builder.Entity<Package>()
                .Property(e => e.Scopes)
                .HasConversion(v => string.Join('|', v),
                    v => v.Split('|', StringSplitOptions.RemoveEmptyEntries));

            builder.Entity<Package>()
                .Property(e => e.Obsoletes)
                .HasConversion(
                    v => v == null || !v.Any() ? null : JsonConvert.SerializeObject(v),
                    v => string.IsNullOrWhiteSpace(v) ? null : JsonConvert.DeserializeObject<List<PackageReference>>(v));

            //builder.Entity<Package>().Ignore(p => p.Templates);

            builder.Entity<Claim>().HasKey(p => p.DatabaseID);
            builder.Entity<Claim>().OwnsOne(p => p.Issuer).HasIndex(i => i.Id);
            builder.Entity<Claim>().OwnsOne(p => p.Subject).HasIndex(i => i.Id);

            builder.Entity<Claim>().HasIndex(p => p.Id).IsUnique(true);

//           builder.Entity<Trust>().HasMany(b => b.Timestamps).WithOne().HasForeignKey(p=>p.ParentID);

            //builder.Entity<Trust>().HasIndex(p => new { p.IssuerAddress, p.SubjectAddress, p.Type, p.Scope }).IsUnique(true);
            builder.Entity<Timestamp>().HasKey(p => p.DatabaseID);
            builder.Entity<Timestamp>().HasIndex(p => p.Source);
            builder.Entity<Timestamp>().HasIndex(p => p.BlockchainProof_db_ID);


            builder.Entity<BlockchainProof>().HasKey(p => p.DatabaseID);

            // Workflow
            builder.Entity<WorkflowContainer>().HasKey(p => p.DatabaseID);
            builder.Entity<WorkflowContainer>().HasIndex(p => p.Type);
            builder.Entity<WorkflowContainer>().HasIndex(p => p.State);

            builder.Entity<KeyValue>().HasIndex(p => p.Key);

            builder.Entity<ClaimPackageRelationship>(relationship => {
                relationship.HasKey(bc => new { bc.ClaimID, bc.PackageID });
                relationship.HasOne(p => p.Claim)
                    .WithMany(x => x.ClaimPackages)
                    .HasForeignKey(y => y.ClaimID)
                    .IsRequired(true);

                relationship.HasOne(p => p.Package)
                    .WithMany(x => x.ClaimPackages)
                    .HasForeignKey(y => y.PackageID)
                    .IsRequired(true); 
            });


            
            base.OnModelCreating(builder);
        }

        public void ClearAllData()
        {
            KeyValues.RemoveRange(KeyValues);
            Workflows.RemoveRange(Workflows);
            Timestamps.RemoveRange(Timestamps);
            Proofs.RemoveRange(Proofs);
            Claims.RemoveRange(Claims);
            Packages.RemoveRange(Packages);

            SaveChanges();
        }

    }
}
