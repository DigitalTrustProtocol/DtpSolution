using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpCore.Extensions
{
    public static class IMediaterExtentions
    {

        public static TResponse SendAndWait<TResponse>(this IMediator mediator, IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return mediator.Send(request, cancellationToken).GetAwaiter().GetResult();
        }
    }
}
