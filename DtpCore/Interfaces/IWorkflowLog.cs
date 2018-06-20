using System;

namespace DtpCore.Interfaces
{
    public interface IWorkflowLog
    {
        string Message { get; set; }
        long Time { get; set; }
        int Count { get; set; }
    }
}