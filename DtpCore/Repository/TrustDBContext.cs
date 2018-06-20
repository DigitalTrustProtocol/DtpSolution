using Microsoft.EntityFrameworkCore;
using DtpCore.Model;

namespace DtpCore.Repository
{
    public class TrustDBContext : DbContext
    {
        public DbSet<Package> Packages { get; set; }
        public DbSet<Trust> Trusts { get; set; }
        public DbSet<Timestamp> Timestamps { get; set; }

        public DbSet<WorkflowContainer> Workflows { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }

        public TrustDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Package>().HasKey(p => p.DatabaseID);
            builder.Entity<Package>().HasAlternateKey(p => p.Id);
            builder.Entity<Package>().OwnsOne(p => p.Server);


            builder.Entity<Trust>().HasKey(p => p.DatabaseID);
            builder.Entity<Trust>().OwnsOne(p => p.Issuer).HasIndex(i => i.Address);
            builder.Entity<Trust>().OwnsOne(p => p.Subject).HasIndex(i => i.Address);
            builder.Entity<Trust>().OwnsOne(p => p.Scope);

            builder.Entity<Trust>().HasIndex(p => p.Id).IsUnique(true);

            //builder.Entity<Trust>().HasIndex(p => p.Issuer.Address );
            //builder.Entity<Trust>().HasIndex(p => p.Subject.Address );

 //           builder.Entity<Trust>().HasMany(b => b.Timestamps).WithOne().HasForeignKey(p=>p.ParentID);

            //builder.Entity<Trust>().HasIndex(p => new { p.IssuerAddress, p.SubjectAddress, p.Type, p.Scope }).IsUnique(true);
            builder.Entity<Timestamp>().HasKey(p => p.DatabaseID);
            builder.Entity<Timestamp>().HasIndex(p => p.Source);
            builder.Entity<Timestamp>().HasIndex(p => p.WorkflowID);

            // Workflow
            builder.Entity<WorkflowContainer>().HasKey(p => p.DatabaseID);
            builder.Entity<WorkflowContainer>().HasIndex(p => p.Type);
            builder.Entity<WorkflowContainer>().HasIndex(p => p.State);


            builder.Entity<KeyValue>().HasIndex(p => p.Key);

            base.OnModelCreating(builder);
        }

        public void ClearAllData()
        {
            KeyValues.RemoveRange(KeyValues);
            Workflows.RemoveRange(Workflows);
            Timestamps.RemoveRange(Timestamps);
            Trusts.RemoveRange(Trusts);
            Packages.RemoveRange(Packages);

            SaveChanges();
        }

    }
}
