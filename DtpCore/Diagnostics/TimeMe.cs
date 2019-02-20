using System;
using System.Diagnostics;

namespace DtpCore.Service
{
    public class TimeMe : IDisposable
    {
        public Stopwatch Watch { get; set; }
        public string Message { get; set; }
        private Action<string> cb;

        public TimeMe(string message, Action<string> callback = null)
        {
            cb = callback ?? Print;

            Message = message;
            Watch = new Stopwatch();
            Watch.Start();

        }

        public void Dispose()
        {
            Watch.Stop();
            cb(Message + " - Elapsed milliseconds: " + Watch.ElapsedMilliseconds + " ");
        }

        public void Print(string message)
        {
            Console.WriteLine(message);
        }
    }
}
