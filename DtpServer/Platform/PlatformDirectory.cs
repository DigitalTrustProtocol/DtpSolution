using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DtpServer.Platform
{
    public class PlatformDirectory
    {
        public string ExecutingPath { get; }
        public string LocalAppDataPath { get; }
        public string DtpServerDataPath { get; }
        public string DatabaseDataPath { get; }

        public PlatformDirectory()
        {
            ExecutingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            LocalAppDataPath = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LocalAppData" : "Home");
            DtpServerDataPath = Path.Combine(LocalAppDataPath, "DtpServer");
            DatabaseDataPath = Path.Combine(DtpServerDataPath, "Database");
        }

        public void EnsureDtpServerDirectory()
        {
            if (!Directory.Exists(DtpServerDataPath))
                Directory.CreateDirectory(DtpServerDataPath);

            if (!Directory.Exists(DatabaseDataPath))
                Directory.CreateDirectory(DatabaseDataPath);
        }
    }
}
