// Source:
// https://blog.maartenballiauw.be/post/2017/08/01/building-a-scheduled-cache-updater-in-aspnet-core-2.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Enumerations;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Strategy.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DtpCore.Services
{
    public class SchedulerHostedService : BackgroundService
    {
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks, IServiceScopeFactory serviceScopeFactory)
        {
            var referenceTime = DateTime.UtcNow;

            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            IList<WorkflowContainer> workflowContainers;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                workflowContainers = workflowService.GetRunningWorkflows();
             }

            foreach (var container in workflowContainers)
            {
                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                                var workflowInstance = workflowService.Create(container);

                                var t = workflowInstance.GetType();
                                var method = t.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                                var arguments = method.GetParameters()
                                                .Select(a => a.ParameterType == typeof(CancellationToken) ? cancellationToken : scope.ServiceProvider.GetService(a.ParameterType))
                                                .ToArray();


                                container.State = WorkflowStatusType.Running.ToString();

                                //invoke.
                                if (typeof(Task).Equals(method.ReturnType))
                                {
                                    await (Task)method.Invoke(workflowInstance, arguments);
                                }
                                else
                                {
                                    method.Invoke(workflowInstance, arguments);
                                }


                                if (container.State == WorkflowStatusType.Running.ToString())
                                {
                                    container.State = WorkflowStatusType.Waiting.ToString();
                                    workflowService.Save(workflowInstance);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public CrontabSchedule Schedule { get; set; }
            public IScheduledTask Task { get; set; }

            public DateTime LastRunTime { get; set; }
            public DateTime NextRunTime { get; set; }

            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }

            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime;
            }
        }
    }
}