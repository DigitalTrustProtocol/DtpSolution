using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using MediatR;
using DtpCore.Extensions;
using DtpCore.Commands.Workflow;
using DtpCore.ViewModel;

namespace DtpServer.Pages.Workflows
{
    public class DetailsModel : PageModel
    {
        public WorkflowView View { get; set; }

        private readonly TrustDBContext _context;
        private readonly IMediator _mediator;

        public DetailsModel(IMediator mediator, TrustDBContext context)
        {
            _mediator = mediator;
            _context = context;
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _mediator.Send(new WorkflowViewQuery {  DatabaseID = id });
            View = result.FirstOrDefault();

            if (View == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
