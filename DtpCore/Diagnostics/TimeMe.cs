using System;
using System.Diagnostics;

namespace DtpCore.Service
{
    public class TimeMe : IDisposable
    {
        public Stopwatch Watch { get; set; }
        public string Message { get; set; }
        private Action<string> _print;

        public TimeMe(string message, Action<string> callback = null)
        {
            _print = callback ?? Print;

            Message = message;
            Watch = new Stopwatch();
            Watch.Start();

        }

        public void Dispose()
        {
            Watch.Stop();

            Trace.WriteLine("Trace line!");
            _print(Message + " - Elapsed milliseconds: " + Watch.ElapsedMilliseconds + " ");

        }

        public static TimeMe Track(string message, Action<string> print = null)
        {
            return new TimeMe(message, print);
        }

        public void Print(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }
}
