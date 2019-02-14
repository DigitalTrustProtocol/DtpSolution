using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DtpServer.Platform.IPFS
{
    public class IpfsManager : IDisposable
    {
        public string ExecutingPath { get; }

        public string LocalAppDataPath { get; }
        public string DataPath { get; }
        public string IpfsDataPath { get; }

        public string IpfsExePath { get; }
        //public string DatabasePath { get; set; }

        private Shell shell;
        private readonly ILogger<IpfsManager> _logger;

        public IpfsManager(ILogger<IpfsManager> logger)
        {
            _logger = logger;

            ExecutingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            LocalAppDataPath = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LocalAppData" : "Home");

            DataPath = Path.Combine(LocalAppDataPath, "DtpServer");
            IpfsDataPath = Path.Combine(DataPath, ".ipfs");
            //DatabasePath = Path.Combine(DataPath, "Database");

            IpfsExePath = Path.Combine(ExecutingPath, @"Platform\ipfs");
        }

        private void EnsureIpfsDir()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            if (Directory.Exists(IpfsDataPath))
                return;

            var local = new Shell(_logger);
            local.ExecuteInline($"ipfsinit.cmd", IpfsDataPath, IpfsExePath);

            CopyToIpfs("swarm.key"); // Copy the DTP network swarm key
                                     //CopyToLocalAppData("config"); // Contains settings regarind bootstrap ip addresses. Maybe not an good idea.
            //Directory.SetCurrentDirectory(savepath);
        }

        public void StartIpfs()
        {
            if (!Directory.Exists(IpfsDataPath))
            {
                EnsureIpfsDir();
                //throw new ApplicationException($"Missing IPFS data directory. {IpfsDataPath} not found.");
            }
            shell = new Shell(_logger);

            shell.StartShell($"ipfsrun.cmd", IpfsDataPath, IpfsExePath);
        }
        
        private void CopyToIpfs(string file)
        {
            File.Copy(Path.Combine(IpfsExePath, file), Path.Combine(IpfsDataPath, file));
        }

        public void Dispose()
        {
            shell?.Dispose();
        }
    }
}
