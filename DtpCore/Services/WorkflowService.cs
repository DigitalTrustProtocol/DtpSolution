using System;
using System.Linq;
using Newtonsoft.Json;
using DtpCore.Interfaces;
using DtpCore.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Enumerations;
using Newtonsoft.Json.Serialization;
using DtpCore.Extensions;
using DtpCore.Workflows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace DtpCore.Services
{
    public class WorkflowService : IWorkflowService
    {
        private ITrustDBService _trustDBService;
        private IExecutionSynchronizationService _executionSynchronizationService;
        public IServiceProvider ServiceProvider { get; set; }
        private IContractResolver _contractResolver;
        private IContractReverseResolver _contractReverseResolver;
        private ILogger _logger;
        private IConfiguration _configuration;

        private Dictionary<string, bool> runningWorkflows = new Dictionary<string, bool>();


        public IQueryable<WorkflowContainer> Workflows {
            get
            {
                return _trustDBService.Workflows;
            }
        }

        public WorkflowService(ITrustDBService trustDBService, IContractResolver contractResolver, IContractReverseResolver contractReverseResolver, 
            IExecutionSynchronizationService executionSynchronizationService, ILogger<WorkflowService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _trustDBService = trustDBService;
            _trustDBService.ID = DateTime.Now.Ticks;

            _executionSynchronizationService = executionSynchronizationService;
            _contractResolver = contractResolver;
            _contractReverseResolver = contractReverseResolver;
            _logger = logger;
            _configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public T Load<T>(int id) where T : class, IWorkflowContext
        {
            var container = Workflows.FirstOrDefault(p => p.DatabaseID == id);
            if (container == null)
                return default(T);
            var workflow = Create<T>(container);
            return workflow;
        }

        public T Create<T>(WorkflowContainer container) where T : class, IWorkflowContext
        {
            var instance = JsonConvert.DeserializeObject<T>(container.Data);
            instance.Container = container;
            instance.WorkflowService = this;

            return instance;
        }

        public T Create<T>() where T : class, IWorkflowContext
        {
            T instance =  (T)Activator.CreateInstance<T>();
            instance.WorkflowService = this;

            return instance;
        }

        public IWorkflowContext Create(WorkflowContainer container)
        {
            var settings = new JsonSerializerSettings();
            var type = Type.GetType(container.Type);
            var instance = (IWorkflowContext)JsonConvert.DeserializeObject(container.Data, type, settings);
            instance.Container = container;
            instance.WorkflowService = this;

            return instance;

        }



        public int Save(IWorkflowContext workflow)
        {
            workflow.UpdateContainer();

            if (workflow.Container.DatabaseID != 0)
            {
                return _trustDBService.DBContext.SaveChanges();
            }
            else
            {
                _trustDBService.DBContext.Workflows.Add(workflow.Container);
                return _trustDBService.DBContext.SaveChanges();
            }
        }

        //public WorkflowContainer CreateWorkflowContainer(IWorkflowContext workflow)
        //{
        //    var entity = new WorkflowContainer
        //    {
        //        Type = workflow.GetType().FullName,
        //        State = workflow.Container.State,
        //        Tag = workflow.Container.Tag,
        //        Data = JsonConvert.SerializeObject(workflow)
        //    };
        //    return entity;
        //}

        public void RunWorkflows()
        {
                var containers = GetRunningWorkflows();

                if (containers.Count > 0)
                {
                    _logger.DateInformation($"Active Workflows found: {containers.Count}");

                    foreach (var container in containers)
                    {
                        // Run workflows serialized! May still be problems with the async parallel processing
                        Execute(container);
                    }

                    _logger.DateInformation($"TaskProcessor done");
                }

            }

        public IList<WorkflowContainer> GetRunningWorkflows()
        {
            var time = DateTime.Now.ToUnixTime();

            var containers = (from p in _trustDBService.Workflows
                              where p.Active && p.NextExecution <= time
                              select p).ToArray();

            return containers;
        }

        //public async void ExecuteAsync(ConcurrentDictionary<string, bool> workflows, WorkflowContainer container)
        //{
        //    if (workflows.ContainsKey(container.Type))
        //        return;

        //    if (!workflows.TryAdd(container.Type, true))
        //        return;

        //    _logger.DateInformation($"Executing workflow id : {container.DatabaseID}");

        //    await Task.Run(() => {
        //        // Make a scope for the workflow to run in.
        //        //var taskServiceProvider = services.BuildServiceProvider();
        //        using (var taskScope = ServiceProvider.CreateScope())
        //        {
        //            var taskWorkflowService = taskScope.ServiceProvider.GetRequiredService<IWorkflowService>();
        //            var workflow = taskWorkflowService.Create(container);

        //            workflow.Execute();
        //        }
        //    });

        //    workflows.TryRemove(container.Type, out bool val);

        //    _logger.DateInformation($"ContinueWith -> Done executing workflow id {container.DatabaseID}");
        //}

        public void Execute(WorkflowContainer container)
        {
            // Make sure that only one of the same type of workflow can run 
            if (runningWorkflows.ContainsKey(container.Type))
                return;

            if (!runningWorkflows.TryAdd(container.Type, true))
                return;

            _logger.DateInformation($"Executing workflow id : {container.DatabaseID}");

            var workflow = Create(container);
            workflow.Container.State = WorkflowStatusType.Running.ToString();
            _trustDBService.DBContext.SaveChanges(); // Update state
            workflow.Execute();

            runningWorkflows.Remove(container.Type, out bool val);

            _logger.DateInformation($"ContinueWith -> Done executing workflow id {container.DatabaseID}");
        }


        public T EnsureWorkflow<T>() where T : class, IWorkflowContext
        {
            var container = Workflows.FirstOrDefault(p => p.Type == typeof(T).AssemblyQualifiedName
                                 && p.Active == true);

            if (container == null)
            {
                var wf = Create<T>();
                Save(wf);
                return wf;
            }

            var runningWf = (T)Create(container);
            return runningWf;
        }

        public void Dispose()
        {
            if (_trustDBService != null)
                _trustDBService.DBContext.Dispose();
        }
    }
}

