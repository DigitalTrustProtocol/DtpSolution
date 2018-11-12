using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;

namespace DtpServer.Pages.Proofs
{
    public class DetailsModel : PageModel
    {
        private readonly DtpCore.Repository.TrustDBContext _context;

        public DetailsModel(DtpCore.Repository.TrustDBContext context)
        {
            _context = context;
        }

        public BlockchainProof BlockchainProof { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlockchainProof = await _context.Proofs.FirstOrDefaultAsync(m => m.DatabaseID == id);

            if (BlockchainProof == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
