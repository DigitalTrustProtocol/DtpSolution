using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using DtpCore.Model;
using DtpCore.Services;
using DtpCore.Workflows;

namespace DtpCore.Interfaces
{
    public interface IWorkflowContext 
    {
        [JsonIgnore]
        IWorkflowService WorkflowService { get; set; }

        [JsonIgnore]
        WorkflowContainer Container { get; set; }

        List<WorkflowLog> Logs { get; set; }

        void Execute();
        void UpdateContainer();
        void Wait(int seconds);
        void Success(string state);
        void Failed(Exception ex);
        void Log(string message);
        
    }
}