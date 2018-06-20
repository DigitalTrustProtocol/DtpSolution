using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpServer.Extensions;


namespace DtpServer.Pages.Workflows
{
    public class CreateModel : PageModel
    {
        private readonly DtpCore.Repository.TrustDBContext _context;

        public CreateModel(DtpCore.Repository.TrustDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            if (!HttpContext.Session.Get<bool>("isadmin"))
                return NotFound();

            return Page();
        }

        [BindProperty]
        public WorkflowContainer WorkflowContainer { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.Get<bool>("isadmin"))
                return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Workflows.Add(WorkflowContainer);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}