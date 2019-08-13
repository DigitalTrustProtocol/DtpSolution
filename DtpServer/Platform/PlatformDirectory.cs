using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DtpServer.Platform
{
    public class PlatformDirectory
    {
        public const string ServerKeywordFilename = "ServerKeyword.json";

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
