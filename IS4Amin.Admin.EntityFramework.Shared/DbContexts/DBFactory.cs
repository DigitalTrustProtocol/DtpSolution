using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace IS4Amin.Admin.EntityFramework.Shared.DbContexts
{
    public class TrustDBContextFactory : IDesignTimeDbContextFactory<AdminIdentityDbContext>
    {
        public AdminIdentityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdminIdentityDbContext>();
            optionsBuilder.UseSqlite("Filename=c://tmp/AdminIdentity.db"); // , b => b.MigrationsAssembly("DtpCore")

            return new AdminIdentityDbContext(optionsBuilder.Options);
        }
    }

    public class AdminLogDbContextFactory : IDesignTimeDbContextFactory<AdminLogDbContext>
    {
        public AdminLogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdminLogDbContext>();
            optionsBuilder.UseSqlite("Filename=c://tmp/AdminLog.db");  // b => b.MigrationsAssembly("IS4Amin.Admin.EntityFramework.Shared")

            return new AdminLogDbContext(optionsBuilder.Options);
        }
    }

    public class IdentityServerConfigurationDbContextFactory : IDesignTimeDbContextFactory<IdentityServerConfigurationDbContext>
    {
        public IdentityServerConfigurationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityServerConfigurationDbContext>();
            optionsBuilder.UseSqlite("Filename=c://tmp/IdentityServerConfiguration.db");

            var op = new ConfigurationStoreOptions();

            return new IdentityServerConfigurationDbContext(optionsBuilder.Options, op);
        }
    }


    public class IdentityServerPersistedGrantDbContextFactory : IDesignTimeDbContextFactory<IdentityServerPersistedGrantDbContext>
    {
        public IdentityServerPersistedGrantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityServerPersistedGrantDbContext>();
            optionsBuilder.UseSqlite("Filename=c://tmp/PersistedGrant.db");
            var op = new OperationalStoreOptions();
            
            return new IdentityServerPersistedGrantDbContext(optionsBuilder.Options, op);
        }
    }

}
