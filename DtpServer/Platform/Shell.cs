﻿using System;
using System.Diagnostics;
using System.IO;
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


        private ProcessStartInfo CreateInlineInfo(string command, string arguments)
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

        private ProcessStartInfo CreateShellExecuteInfo(string command, string arguments, string workingDirectory)
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


        public void ExecuteInline(string command, string arguments, string workingDirectory)
        {
            var temp = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workingDirectory);

            Start(CreateInlineInfo(command, arguments));
            WaitForExit();

            Directory.SetCurrentDirectory(temp);
        }

        public void StartShell(string command, string arguments, string workingDirectory)
        {
            Start(CreateShellExecuteInfo(command, arguments, workingDirectory));
        }

        private Process Start(ProcessStartInfo info)
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
                        //_logger.LogInformation(data.Data);
                        //Console.WriteLine(data.Data);
                        Log.Information(data.Data);
                    };
                    
                    process.ErrorDataReceived += (sender, data) => {
                        //_logger.LogInformation(data.Data);
                        //Console.WriteLine(data.Data);
                        Log.Error(data.Data);
                    };
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
                process.CloseMainWindow();
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