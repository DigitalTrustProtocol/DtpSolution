using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using System.Linq;
using DtpCore.Model.Database;

namespace DtpServer.Pages.Packages
{
    public class IndexModel : PageModel
    {
        private readonly TrustDBContext _context;

        public IndexModel(TrustDBContext context)
        {
            _context = context;
        }

        public IList<Package> BuildPackages { get; set; }
        public IList<Package> Packages { get;set; }

        /// <summary>
        /// Load data
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            BuildPackages = await _context.Packages.AsNoTracking().Where(p => p.State == PackageStateType.Building).ToListAsync();
            foreach (var package in BuildPackages)
            {
                if (string.IsNullOrEmpty(package.Scopes))
                    package.Scopes = "Global";
            }

            Packages = await _context.Packages.AsNoTracking().Where(p => p.State != PackageStateType.Building).ToListAsync();
            foreach (var package in Packages)
            {
                if (string.IsNullOrEmpty(package.Scopes))
                    package.Scopes = "Global";
            }
        }
    }
}
