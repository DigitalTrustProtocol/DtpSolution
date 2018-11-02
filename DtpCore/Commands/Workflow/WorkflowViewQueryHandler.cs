using DtpCore.Collections.Generic;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.ViewModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Commands.Workflow
{
    public class WorkflowViewQueryHandler : IRequestHandler<WorkflowViewQuery, IPaginatedList<WorkflowView>>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<WorkflowViewQueryHandler> _logger;

        public object PaginatedList { get; private set; }

        public WorkflowViewQueryHandler(IMediator mediator, TrustDBContext db, ILogger<WorkflowViewQueryHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<IPaginatedList<WorkflowView>> Handle(WorkflowViewQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Workflows.AsNoTracking().Select(p => CreateView(p));

            if(request.DatabaseID != null)
                query = query.Where(p => p.DatabaseID == request.DatabaseID);

            var list = PaginatedList<WorkflowView>.CreateAsync(query, request.PageIndex.GetValueOrDefault(), request.PageSize.GetValueOrDefault() );

            // There can be more of the same type.
            //var query = _context.Workflows.GroupBy(p => p.Type).Select(p => p.OrderByDescending(t => t.DatabaseID).First());

            return Task.FromResult((IPaginatedList<WorkflowView>)list.GetAwaiter().GetResult());
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
                Tag = container.Tag,
                Data = container.Data,
                NextExecution = DatetimeExtensions.FromUnixTime(container.NextExecution)
            };

            return view;
        }

    }
}
