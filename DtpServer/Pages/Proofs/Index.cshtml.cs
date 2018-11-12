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
    public class IndexModel : PageModel
    {
        private readonly DtpCore.Repository.TrustDBContext _context;

        public IndexModel(DtpCore.Repository.TrustDBContext context)
        {
            _context = context;
        }

        public IList<BlockchainProof> BlockchainProof { get;set; }

        public async Task OnGetAsync()
        {
            BlockchainProof = await _context.Proofs.ToListAsync();
        }
    }
}
