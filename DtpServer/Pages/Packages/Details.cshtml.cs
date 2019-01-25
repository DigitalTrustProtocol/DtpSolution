using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Collections.Generic;
using DtpCore.Interfaces;

namespace DtpServer.Pages.Packages
{
    public class DetailsModel : PageModel
    {
        private readonly ITrustDBService _trustDBService;

        public DetailsModel(ITrustDBService trustDBService)
        {
            _trustDBService = trustDBService;
        }

        public Package Package { get; set; }
        public PaginatedList<Claim> Claims { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }



            Package = await _trustDBService.DBContext.Packages.Include(p=>p.Timestamps).FirstOrDefaultAsync(m => m.DatabaseID == id);

            if (Package == null)
            {
                return NotFound();
            }


            _trustDBService.LoadPackageClaims(Package);

            var query = _trustDBService.DBContext.ClaimPackageRelationships.Where(p => p.PackageID == id).Include(p => p.Claim).OrderBy(p => p.Claim.Created).Select(p => p.Claim);
            Claims = await PaginatedList<Claim>.CreateAsync(query, 1, 0); // PageSize = 0 is unlimited

            return Page();
        }
    }
}
