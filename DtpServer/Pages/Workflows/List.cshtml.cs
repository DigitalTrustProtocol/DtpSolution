using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Services;
using Microsoft.Extensions.Configuration;
using DtpServer.AspNetCore.Mvc.RazorPages;
using DtpCore.Collections.Generic;
using System.Collections;
using DtpCore.Workflows;

namespace DtpServer.Pages.Workflows
{
    public class ListModel : ListPageModel<WorkflowContainer>
    {
        private readonly TrustDBContext _context;
        private readonly IWorkflowService _workflowService;

        public string WorkflowType { get; set; }

        public ListModel(TrustDBContext context, IWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;
        }

        public IList<WorkflowContainer> WorkflowContainer { get;set; }



        public async Task OnGetAsync(string type, string sortOrder, string sortField, string currentFilter, string searchString, int? pageIndex)
        {
            WorkflowType = type;
            InitProperties(sortOrder, sortField, currentFilter, searchString, pageIndex);

            var query = from p in _context.Workflows
                        where p.Type == type
                        select p;


            query = BuildQuery(CurrentFilter, query);


            query = AddSorting(query, "DatabaseID", "_desc");

            List = await PaginatedList<WorkflowContainer>.CreateAsync(query.AsNoTracking(), PageIndex, PageSize);
        }


        public async Task OnGetRunNowAsync(int id, string sortOrder, string sortField, string currentFilter, string searchString, int? pageIndex)
        {
            var container = await _context.Workflows.FirstOrDefaultAsync(p => p.DatabaseID == id);

            var wf = _workflowService.Create<WorkflowContext>(container);
            wf.Container.NextExecution = DateTime.Now.ToUnixTime();
            wf.Container.State = WorkflowStatusType.Starting.ToString();
            _workflowService.Save(wf);

            await OnGetAsync(container.Type, sortOrder, sortField, currentFilter, searchString, pageIndex);
        }

        public bool ShowRunNow(WorkflowContainer container)
        {
            return WorkflowStatusType.New.ToString().EqualsIgnoreCase(container.State);
        }

        private IQueryable<WorkflowContainer> BuildQuery(string searchString, IQueryable<WorkflowContainer> query)
        {
            if (String.IsNullOrEmpty(searchString))
                return query;

            if (int.TryParse(searchString, out int workflowId))
                query = query.Where(s => s.DatabaseID== workflowId);

            return query;
        }

    }
}
