using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Extensions;

namespace DtpCore.Services
{
    /// <summary>
    /// Depricated, replaced by SchedulerHostedService for better async handling.
    /// </summary>
    public class WorkflowHostedService : BackgroundService //IHostedService
    {
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        //private Timer _timer;

        public WorkflowHostedService(IServiceProvider services, ILogger<WorkflowHostedService> logger, IConfiguration configuration)
        {
            Services = services;
            _logger = logger;
            _configuration = configuration;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"Workflow Hosted Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = Services.CreateScope())
                {
                    var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                    {
                        //workflowService.RunWorkflows();
                    }
                }
                await Task.Delay(_configuration.WorkflowInterval()*1000, stoppingToken); // WorkflowInterval is in seconds
            }

            _logger.LogDebug($"Workflow Hosted Service is stopping.");
        }

        //protected override async Task StopAsync(CancellationToken stoppingToken)
        //{
        //    // Run your graceful clean-up actions
        //}

        //public Task StartAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation(
        //        "Workflow Hosted Service is starting.");

        //    _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(_configuration.WorkflowInterval()));

        //    return Task.CompletedTask;
        //}

        //private void DoWork(object state)
        //{
        //    using (var scope = Services.CreateScope())
        //    {
        //        var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
        //        {
        //            workflowService.RunWorkflows();
        //        }
        //    }
        //}

        //public Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation(
        //        "Workflow Hosted Service is stopping.");

        //    return Task.CompletedTask;
        //}
    }
}
