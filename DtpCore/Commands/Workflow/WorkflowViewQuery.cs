using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.ViewModel;
using MediatR;

namespace DtpCore.Commands.Workflow
{
    public class WorkflowViewQuery : QueryCommand, IRequest<IPaginatedList<WorkflowView>>
    {
        public int? DatabaseID { get; set; }
    }
}
