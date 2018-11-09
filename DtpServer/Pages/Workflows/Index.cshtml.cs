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

namespace DtpServer.Pages.Workflows
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly TrustDBContext _context;

        public IList<WorkflowView> WorkflowViews { get; set; }

        public IndexModel(IMediator mediator, TrustDBContext context)
        {
            _mediator = mediator;
            _context = context;
        }


        public void OnGet()
        {
            WorkflowViews = _mediator.SendAndWait(new WorkflowViewQuery());

        }
    }
}
