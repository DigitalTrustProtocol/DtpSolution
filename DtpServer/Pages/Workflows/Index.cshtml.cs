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
using DtpCore.Commands.Workflow;
using DtpCore.Extensions;
using DtpCore.ViewModel;
using DtpCore.Enumerations;
using Microsoft.Extensions.Hosting;
using DtpCore.Services;

namespace DtpServer.Pages.Workflows
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly TrustDBContext _context;
        private readonly IHostedService _schedulerHostedService;

        public IList<WorkflowView> WorkflowViews { get; set; }
        public bool Admin { get; set; }

        [BindProperty]
        public int DatabaseID { get; set; }

        public IndexModel(IMediator mediator, TrustDBContext context, IHostedService schedulerHostedService)
        {
            _mediator = mediator;
            _context = context;
            _schedulerHostedService = schedulerHostedService;
#if DEBUG
            Admin = true;
#endif
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            WorkflowViews = _mediator.SendAndWait(new WorkflowViewQuery());

#if DEBUG
            if (id != null)
            {
                var workflowContainer = await _context.Workflows.SingleOrDefaultAsync(m => m.DatabaseID == id.Value);
                workflowContainer.NextExecution = 1;
                workflowContainer.State = WorkflowStatusType.Running.ToString();
                _context.SaveChanges();
                //SchedulerHostedService.RunNow();
                ((SchedulerHostedService)_schedulerHostedService).RunNow();
                return Redirect("./Workflows");
            }
#endif

            return Page();
        }
    }
}
