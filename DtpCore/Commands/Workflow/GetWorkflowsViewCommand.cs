using DtpCore.ViewModel;
using MediatR;
using System.Collections.Generic;

namespace DtpCore.Commands.Workflow
{
    public class GetWorkflowsViewCommand : IRequest<IList<WorkflowView>>
    {

    }
}
