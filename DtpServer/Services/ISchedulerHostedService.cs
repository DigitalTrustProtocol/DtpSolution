// Source:
// https://blog.maartenballiauw.be/post/2017/08/01/building-a-scheduled-cache-updater-in-aspnet-core-2.html

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Model;

namespace DtpServer.Services
{
    public interface ISchedulerHostedService
    {
        event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        Task RunContainer(IServiceProvider serviceProvider, WorkflowContainer container, CancellationToken cancellationToken);
        Func<Task> RunContainers(IList<WorkflowContainer> containers, CancellationToken cancellationToken);
        void RunNow(int containerId);
        Task StartAsync(CancellationToken cancellationToken);
    }
}