// Source:
// https://blog.maartenballiauw.be/post/2017/08/01/building-a-scheduled-cache-updater-in-aspnet-core-2.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Services;
using DtpCore.Strategy.Cron;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DtpServer.Services
{
    public class SchedulerHostedService : BackgroundService, ISchedulerHostedService
    {
        public static CancellationTokenSource DelayTokenSource = null;

        public int ContainerId = 0;

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;


        public void RunNow(int containerId)
        {
            DelayTokenSource = DelayTokenSource ?? new CancellationTokenSource();
            ContainerId = containerId;
            ExecuteOne(DelayTokenSource.Token);
        }

        public SchedulerHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<SchedulerHostedService> logger, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _configuration = configuration;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            DelayTokenSource = new CancellationTokenSource();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace($"Scheduler Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                ExecuteOnce(cancellationToken);

                await Task.Delay(_configuration.WorkflowInterval() * 1000, DelayTokenSource.Token);
                //_logger.LogInformation("Checking Workflows !");
                if (DelayTokenSource.IsCancellationRequested)
                    DelayTokenSource = new CancellationTokenSource();
            }

            _logger.LogTrace($"Scheduler Service is stopping.");
        }

        private void ExecuteOnce(CancellationToken cancellationToken)
        {

            if (ContainerId > 0)
                return; // Do not execute now!

            // Get all workflows group by type.
            var workflowContainers = GetWorkflowContainers();

            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var list = new List<Task>();
            foreach (var entry in workflowContainers)
            {
                list.Add(taskFactory.StartNew(RunContainers(entry.Value, cancellationToken)));
            }
            Task.WaitAll(list.ToArray()); // Do not leave before every workflow is done!
        }

        private void ExecuteOne(CancellationToken cancellationToken)
        {
            try
            {

                // Run only one specific workflow
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                    var db = scope.ServiceProvider.GetRequiredService<TrustDBContext>();
                    var workflowContainer = db.Workflows.SingleOrDefaultAsync(m => m.DatabaseID == ContainerId).GetAwaiter().GetResult();

                    RunContainer(scope.ServiceProvider, workflowContainer, cancellationToken).GetAwaiter().GetResult();
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
            finally
            {
                ContainerId = 0;
            }
        }

        private Dictionary<string, IList<WorkflowContainer>> GetWorkflowContainers()
        {
            var workflowContainers = new Dictionary<string, IList<WorkflowContainer>>();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var list = workflowService.GetRunningWorkflows();

                foreach (var item in list)
                {
                    if (!workflowContainers.ContainsKey(item.Type))
                        workflowContainers[item.Type] = new List<WorkflowContainer>();

                    workflowContainers[item.Type].Add(item);
                }
            }

            return workflowContainers;
        }

        public Func<Task> RunContainers(IList<WorkflowContainer> containers, CancellationToken cancellationToken)
        {
            return async () =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {

                        // Run all the same workflow type in a sequential order
                        foreach (var container in containers)
                        {
                            await RunContainer(scope.ServiceProvider, container, cancellationToken);
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
            };
        }

        public async Task RunContainer(IServiceProvider serviceProvider, WorkflowContainer container, CancellationToken cancellationToken)
        {
            var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();
            var workflowInstance = workflowService.Create(container);

            var t = workflowInstance.GetType();
            var method = t.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
            var arguments = method.GetParameters()
                            .Select(a => a.ParameterType == typeof(CancellationToken) ? cancellationToken : serviceProvider.GetService(a.ParameterType))
                            .ToArray();

            try
            {
                container.State = WorkflowStatusType.Running.ToString();
                workflowService.Save(workflowInstance);

                if (typeof(Task).Equals(method.ReturnType))
                {
                    await (Task)method.Invoke(workflowInstance, arguments);
                }
                else
                {
                    method.Invoke(workflowInstance, arguments);
                }
            }
            finally
            {
                if (container.State == WorkflowStatusType.Running.ToString())
                {
                    container.State = WorkflowStatusType.Waiting.ToString();
                    workflowService.Save(workflowInstance);
                }
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