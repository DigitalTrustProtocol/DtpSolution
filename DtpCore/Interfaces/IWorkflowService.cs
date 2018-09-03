using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DtpCore.Interfaces;
using DtpCore.Model;

namespace DtpCore.Services
{
    public interface IWorkflowService : IDisposable
    {
        //ITrustDBService DBService { get; set; }
        [JsonIgnore]
        IServiceProvider ServiceProvider { get; set; }
        IQueryable<WorkflowContainer> Workflows { get;  }

        T Load<T>(int id) where T : class, IWorkflowContext;
        //IWorkflowContext Create(WorkflowContainer container);
        T Create<T>(WorkflowContainer container) where T : class, IWorkflowContext;
        T Create<T>() where T : class, IWorkflowContext;
        IWorkflowContext Create(WorkflowContainer container);
        //void Execute(Dictionary<string, bool> workflows, WorkflowContainer container, );
        int Save(IWorkflowContext workflow);
        //WorkflowContainer Load();
        //WorkflowContainer CreateWorkflowContainer(IWorkflowContext workflow);
        IList<WorkflowContainer> GetRunningWorkflows();
        //void RunWorkflows();
        //void Execute(WorkflowContainer container);
        //void ExecuteAsync(ConcurrentDictionary<string, bool> workflows, WorkflowContainer container);
        //T GetRunningWorkflow<T>() where T : class, IWorkflowContext;
        T EnsureWorkflow<T>() where T : class, IWorkflowContext;
    }
}