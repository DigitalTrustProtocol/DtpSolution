using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DtpServer.Platform.IPFS
{
    public class IpfsManager : IDisposable
    {
        
        public string IpfsDataPath { get; }
        public string IpfsExePath { get; }

        private Shell shell;
        private PlatformDirectory platformDirectory;

        private readonly ILogger<IpfsManager> _logger;

        public IpfsManager(PlatformDirectory platform, ILogger<IpfsManager> logger)
        {
            _logger = logger;
            platformDirectory = platform;
            IpfsDataPath = Path.Combine(platform.DtpServerDataPath, ".ipfs");

            IpfsExePath = Path.Combine(platform.ExecutingPath, @"Platform\ipfs");
        }

        private void EnsureIpfsDir()
        {
            platformDirectory.EnsureDtpServerDirectory();

            if (Directory.Exists(IpfsDataPath))
                return;

            var local = new Shell(_logger);
            local.ExecuteInline($"ipfsinit.cmd", IpfsDataPath, IpfsExePath);

            CopyToIpfs("swarm.key"); // Copy the DTP network swarm key
        }

        public void StartIpfs()
        {
            if (!Directory.Exists(IpfsDataPath))
                EnsureIpfsDir();

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
            _logger.LogInformation("IpfsManager disposed");
        }
    }
}
