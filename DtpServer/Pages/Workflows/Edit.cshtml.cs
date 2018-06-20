using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpServer.Extensions;

namespace DtpServer.Pages.Workflows
{
    public class EditModel : PageModel
    {
        private readonly TrustDBContext _context;

        public EditModel(TrustDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkflowContainer WorkflowContainer { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!HttpContext.Session.Get<bool>("isadmin"))
                return NotFound();

            if (id == null)
            {
                return NotFound();
            }

            WorkflowContainer = await _context.Workflows.SingleOrDefaultAsync(m => m.DatabaseID == id);

            if (WorkflowContainer == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(WorkflowContainer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
            }

            return RedirectToPage("./Index");
        }
    }
}
