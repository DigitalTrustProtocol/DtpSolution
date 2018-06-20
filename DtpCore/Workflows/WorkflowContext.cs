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
        public List<IWorkflowLog> Logs { get; set; }

        [JsonIgnore]
        public IWorkflowService WorkflowService { get; set; }

        private Dictionary<string, IWorkflowLog> _logDictionary = null;


        public WorkflowContext() 
        {
            Container = new WorkflowContainer
            {
                Type = GetType().AssemblyQualifiedName,
                State = WorkflowStatusType.New.ToString()
            };
            Logs = new List<IWorkflowLog>();
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
            if(_logDictionary != null)
                Logs = _logDictionary.Values.ToList<IWorkflowLog>();

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
            if(_logDictionary == null)
            {
                _logDictionary = new Dictionary<string, IWorkflowLog>();
                foreach(var l in Logs)
                {
                    if (_logDictionary.ContainsKey(l.Message))
                        continue;

                    _logDictionary.Add(l.Message, l);
                }
            }

            var log = new WorkflowLog { Message = message };
            if (_logDictionary.TryGetValue(log.Message, out IWorkflowLog oldLog))
            {
                log.Count = oldLog.Count + 1;
            }

            _logDictionary[log.Message] = log;
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
