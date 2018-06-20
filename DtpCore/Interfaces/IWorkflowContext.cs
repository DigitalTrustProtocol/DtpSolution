using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtpCore.Model;
using DtpCore.Services;

namespace DtpCore.Interfaces
{
    public interface IWorkflowContext 
    {
        [JsonIgnore]
        IWorkflowService WorkflowService { get; set; }

        [JsonIgnore]
        WorkflowContainer Container { get; set; }

        List<IWorkflowLog> Logs { get; set; }

        void Execute();
        void UpdateContainer();
        void Wait(int seconds);
        void Success(string state);
        void Failed(Exception ex);
        void Log(string message);
        
    }
}