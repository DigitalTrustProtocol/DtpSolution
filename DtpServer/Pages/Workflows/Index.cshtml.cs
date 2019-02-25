using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Repository;
using MediatR;
using DtpCore.Commands.Workflow;
using DtpCore.Extensions;
using DtpCore.ViewModel;
using DtpCore.Enumerations;
using Microsoft.Extensions.Hosting;
using DtpCore.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace DtpServer.Pages.Workflows
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly TrustDBContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostedService _schedulerHostedService;
        private readonly IConfiguration _configuration;

        public IList<WorkflowView> WorkflowViews { get; set; }
        public bool Admin { get; set; }

        [BindProperty]
        public int DatabaseID { get; set; }
        public object Datetime { get; private set; }

        public IndexModel(IMediator mediator, TrustDBContext context, IServiceProvider serviceProvider, IHostedService schedulerHostedService, IConfiguration configuration)
        {
            _mediator = mediator;
            _context = context;
            _serviceProvider = serviceProvider;
            _schedulerHostedService = schedulerHostedService;
            _configuration = configuration;
            Admin = _configuration.IsAdminEnabled(Admin);
        }


        public async Task<IActionResult> OnGetAsync()
        {
            WorkflowViews = _mediator.SendAndWait(new WorkflowViewQuery());

            return Page();
        }

        public async Task<IActionResult> OnGetRunAsync(int id)
        {
            await OnGetAsync();

            if (Admin && id > 0)
            {
                var tokenSource = new CancellationTokenSource();

                ((SchedulerHostedService)_schedulerHostedService).RunNow(id);

                return Redirect("./Workflows");
            }
            return Page();
        }
    }
}
