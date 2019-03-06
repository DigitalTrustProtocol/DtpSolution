using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Serilog;

namespace DtpServer.Platform
{
    public class Shell: IDisposable
    {

        private bool started = false;
        private Process process = null;
        private ProcessStartInfo processInfo;


        public Shell()
        {
        }


        private ProcessStartInfo CreateInlineInfo(string command, string arguments = "")
        {
            var startInfo = new ProcessStartInfo(command, arguments);

            // When UseShellExecute is false, the WorkingDirectory property is not used to find the executable. 
            // Instead, its value applies to the process that is started and only has meaning within the context of the new process.
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            return startInfo;
        }

        private ProcessStartInfo CreateShellExecuteInfo(string command, string arguments = "", string workingDirectory = null)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                workingDirectory = Directory.GetCurrentDirectory();

            var startInfo = new ProcessStartInfo(command, arguments);
            startInfo.WorkingDirectory = workingDirectory;

            // When UseShellExecute is true, the working directory of the application that starts the executable is also the working directory of the executable.
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false;

            return startInfo;
        }


        public Shell ExecuteInline(string command, string arguments ="", string workingDirectory = null, Action<string> outputcallback = null)
        {
            string temp = null;
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                temp = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(workingDirectory);
            }

            var info = CreateInlineInfo(command, arguments);
            Start(info, outputcallback);
            WaitForExit();

            if (temp != null)
                Directory.SetCurrentDirectory(temp);

            return this;
        }

        public void StartShell(string command, string arguments = "", string workingDirectory = null)
        {
            Start(CreateShellExecuteInfo(command, arguments, workingDirectory));
        }

        private Process Start(ProcessStartInfo info, Action<string> outputcallback = null)
        {
            if (!started)
            {
                processInfo = info;
                process = new Process
                {
                    StartInfo = info
                };
                if (!info.UseShellExecute)
                {
                    process.OutputDataReceived += (sender, data) => {
                        Log.Information(data.Data);
                    };
                    
                    process.ErrorDataReceived += (sender, data) => {
                        Log.Error(data.Data);
                    };

                    if (outputcallback != null)
                        process.OutputDataReceived += (sender, data) => outputcallback.Invoke(data.Data);
                }

                Log.Information($"Starting Process {processInfo.FileName} from folder: {processInfo.WorkingDirectory}");
                process.Start();
                if (!info.UseShellExecute)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                Log.Information($"Process {processInfo.FileName} started");
            }

            started = true;
            return process;
        }

        public void WaitForExit()
        {
            if(started)
                process.WaitForExit();
            started = false;
        }

        public void Stop()
        {
            if (started)
            {
                process.Kill();
                Log.Information($"Process {processInfo.FileName} stopped");

            }
            started = false;
        }


        public void Dispose()
        {
            if (started)
            {
                Stop();
                process.Dispose();
                Log.Information($"Process {processInfo.FileName} disposed");
            }
        }
    }
}
