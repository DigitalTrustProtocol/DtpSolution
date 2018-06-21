using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using DtpCore.Enumerations;
using DtpCore.Interfaces;
using DtpCore.Services;
using DtpCore.Extensions;
using DtpCore.Model;
using Microsoft.Extensions.Logging;

namespace DtpCore.Workflows
{
    public class WorkflowContext : IWorkflowContext
    {
        [JsonIgnore]
        public WorkflowContainer Container { get; set; }

        [JsonProperty(PropertyName = "logs", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorkflowLog> Logs { get; set; }

        [JsonIgnore]
        public IWorkflowService WorkflowService { get; set; }

        public WorkflowContext() 
        {
            Container = new WorkflowContainer
            {
                Type = GetType().AssemblyQualifiedName,
                State = WorkflowStatusType.New.ToString()
            };
            Logs = new List<WorkflowLog>();
        }

        public virtual void UpdateContainer()
        {
            Container.Data = JsonConvert.SerializeObject(this);
        }


        public virtual void Execute()
        {
        }

        public virtual void Save()
        {
            WorkflowService.Save(this);
        }

        public virtual void Wait(int seconds)
        {
            Container.NextExecution = DateTime.Now.AddSeconds(seconds).ToUnixTime();
            Container.State = WorkflowStatusType.Waiting.ToString();
            Save();
        }

        public virtual void Success(string state = "Success")
        {
            Container.State = state;
            Container.Active = false;
            Log("Workflow completed successfully");
            Save();
        }

        public virtual void Failed(Exception ex)
        {
            Container.State = "Failed";
            Container.Active = false;

#if DEBUG
            Log($"Error: {ex.Message} - {ex.StackTrace}");
#else
            Log($"Error: {ex.Message}");
#endif            
            Save();

        }

        public virtual void Log(string message)
        {
            if(Logs.Count > 100)
            {
                Logs.RemoveAt(0);
            }
            Logs.Add(new WorkflowLog { Message = message });
        }

        public void CombineLog(ILogger logger, string msg)
        {
            logger.DateInformation(Container.DatabaseID, msg);
            Log(msg);
        }


        public void CallMethod(string name)
        {
            GetType().GetMethod(name).Invoke(this, null);
        }
    }
}
