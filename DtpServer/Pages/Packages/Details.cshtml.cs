using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Collections.Generic;

namespace DtpServer.Pages.Packages
{
    public class DetailsModel : PageModel
    {
        private readonly DtpCore.Repository.TrustDBContext _context;

        public DetailsModel(DtpCore.Repository.TrustDBContext context)
        {
            _context = context;
        }

        public Package Package { get; set; }
        public PaginatedList<Claim> Trusts { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Package = await _context.Packages.FirstOrDefaultAsync(m => m.DatabaseID == id);

            if (Package == null)
            {
                return NotFound();
            }

            var query = _context.Claims.Where(p => p.PackageDatabaseID == id).OrderBy(p=> p.Created);
            Trusts = await PaginatedList<Claim>.CreateAsync(query, 1, 0); // PageSize = 0 is unlimited


            return Page();
        }
    }
}
