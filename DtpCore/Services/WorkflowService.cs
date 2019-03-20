using System;
using System.Linq;
using Newtonsoft.Json;
using DtpCore.Interfaces;
using DtpCore.Model;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using DtpCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using DtpCore.Enumerations;

namespace DtpCore.Services
{
    public class WorkflowService : IWorkflowService
    {
        private ITrustDBService _trustDBService;
        public IServiceProvider ServiceProvider { get; set; }
        private IContractResolver _contractResolver;
        private IContractReverseResolver _contractReverseResolver;
        private ILogger _logger;
        private IConfiguration _configuration;

        public IQueryable<WorkflowContainer> Workflows {
            get
            {
                return _trustDBService.Workflows;
            }
        }

        public WorkflowService(ITrustDBService trustDBService, IContractResolver contractResolver, IContractReverseResolver contractReverseResolver, 
            ILogger<WorkflowService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _trustDBService = trustDBService;
            _trustDBService.ID = DateTime.Now.Ticks;

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
            var t = typeof(T);

            var constructors = t.GetConstructors();

            if(constructors.Length <= 0)
            {
                throw new ApplicationException("Unable to create object without constructor!");
            }
            var item = constructors[0];
            var arguments = item.GetParameters()
                            .Select(a => ServiceProvider.GetService(a.ParameterType))
                            .ToArray();

            T instance = (T)Activator.CreateInstance(t, arguments);

            instance.WorkflowService = this;

            return instance;
        }

        public IWorkflowContext Create(WorkflowContainer container)
        {
            var type = Type.GetType(container.Type);
            var instance = Deserialize(type, container.Data);

            instance.Container = container;
            instance.WorkflowService = this;

            return instance;

        }

        public IWorkflowContext Deserialize<T>(string data)
        {
            return Deserialize(typeof(T), data);
        }

        public IWorkflowContext Deserialize(Type type, string data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = _contractResolver
            };
            var instance = (IWorkflowContext)JsonConvert.DeserializeObject(data, type, settings);
            return instance;
        }

        public int Save(IWorkflowContext workflow)
        {
            workflow.UpdateContainer();

            if (workflow.Container.DatabaseID != 0)
            {
                _trustDBService.DBContext.Workflows.Update(workflow.Container);
                return _trustDBService.DBContext.SaveChanges();
            }
            else
            {
                _trustDBService.DBContext.Workflows.Add(workflow.Container);
                return _trustDBService.DBContext.SaveChanges();
            }
        }


        public IList<WorkflowContainer> GetRunningWorkflows()
        {
            var time = DateTime.Now.ToUnixTime();

            var containers = (from p in _trustDBService.Workflows
                              where p.Active 
                              && p.NextExecution <= time 
                              && p.State != WorkflowStatusType.Running.ToString()
                              select p).ToArray();

            return containers;
        }


        /// <summary>
        /// Ensures that a workflow is installed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

