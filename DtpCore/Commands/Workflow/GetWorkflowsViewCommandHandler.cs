using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.ViewModel;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Workflow
{
    public class GetWorkflowsViewCommandHandler : IRequestHandler<GetWorkflowsViewCommand, IList<WorkflowView>>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<GetWorkflowsViewCommandHandler> _logger;

        public GetWorkflowsViewCommandHandler(IMediator mediator, TrustDBContext db, ILogger<GetWorkflowsViewCommandHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<IList<WorkflowView>> Handle(GetWorkflowsViewCommand request, CancellationToken cancellationToken)
        {
            var views = from p in _db.Workflows
                        select CreateView(p);

            var list = (IList<WorkflowView>)views.ToList();

            // There can be more of the same type.
            //var query = _context.Workflows.GroupBy(p => p.Type).Select(p => p.OrderByDescending(t => t.DatabaseID).First());

            return Task.FromResult(list);
        }

        protected WorkflowView CreateView(WorkflowContainer container)
        {
            var type = Type.GetType(container.Type);
            var view = new WorkflowView
            {
                DatabaseID = container.DatabaseID,
                Title = type.GetDisplayName(),
                Description = type.GetDescription(),
                State = container.State,
                Active = container.Active,
                NextExecution = DatetimeExtensions.FromUnixTime(container.NextExecution)
            };

            return view;
        }

    }
}
