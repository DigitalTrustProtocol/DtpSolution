using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using DtpCore.Commands.Workflow;
using DtpCore.ViewModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DtpServer.Services;
using Microsoft.AspNetCore.Hosting;

namespace DtpServer.Pages.Workflows
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ISchedulerHostedService _schedulerHostedService;
        private readonly IConfiguration _configuration;


        public IList<WorkflowView> WorkflowViews { get; set; }
        public bool Admin { get; set; }

        [BindProperty]
        public int DatabaseID { get; set; }
        public object Datetime { get; private set; }

        public IndexModel(IMediator mediator, ISchedulerHostedService schedulerHostedService, IConfiguration configuration, IWebHostEnvironment env)
        {
            _mediator = mediator;
            _schedulerHostedService = schedulerHostedService;
            _configuration = configuration;
            //Admin = _configuration.IsAdminEnabled(Admin);
            Admin = env.IsDevelopment();
        }


        public async Task<IActionResult> OnGetAsync()
        {
            WorkflowViews = await _mediator.Send(new WorkflowViewQuery());

            return Page();
        }

        public async Task<IActionResult> OnGetRunAsync(int id)
        {
            await OnGetAsync();

            if (Admin && id > 0)
            {
                _schedulerHostedService.RunNow(id);

                return Redirect("./Workflows");
            }
            return Page();
        }
    }
}
