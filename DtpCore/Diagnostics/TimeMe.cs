using System;
using System.Diagnostics;

namespace DtpCore.Service
{
    public class TimeMe : IDisposable
    {
        public Stopwatch Watch { get; set; }
        public string Message { get; set; }

        public TimeMe(string message)
        {
            Message = message;
            Watch = new Stopwatch();
            Watch.Start();
        }

        public void Dispose()
        {
            Watch.Stop();
            Print();
        }

        public virtual void Print()
        {
            Trace.TraceInformation(Message+ " - Elapsed milliseconds: " + Watch.ElapsedMilliseconds + " ");
        }
    }
}
