using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DtpCore.Repository
{
    public class TrustDBContextFactory : IDesignTimeDbContextFactory<TrustDBContext>
    {
        public TrustDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TrustDBContext>();
            optionsBuilder.UseSqlite("Filename=./trust.db", b => b.MigrationsAssembly("DtpCore"));
            
            return new TrustDBContext(optionsBuilder.Options);
        }
    }
}
