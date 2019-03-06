using DtpPackageCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DtpServer.Platform.ipfs
{
    public class IPFSShell : BackgroundService
    {

        private Shell shell = null;
        public string IpfsDataPath { get; set; }
        public string IpfsExePath { get; set; }
        public event EventHandler WaitForInputReady;
        private PlatformDirectory platformDirectory;

        public IPFSShell(PlatformDirectory platformDirectory)
        {
            this.platformDirectory = platformDirectory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            shell = new Shell();
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                IpfsDataPath = Path.Combine(platformDirectory.DtpServerDataPath, ".ipfs");
                IpfsExePath = Path.Combine(platformDirectory.ExecutingPath, @"Platform\ipfs");
                var ipfsexe = Path.Combine(IpfsExePath, "ipfs.exe");

                Environment.SetEnvironmentVariable("IPFS_PATH", IpfsDataPath);
                Environment.SetEnvironmentVariable("LIBP2P_FORCE_PNET", "1");

                if (!Directory.Exists(IpfsDataPath))
                {
                    Directory.CreateDirectory(IpfsDataPath);
                    CopyToIpfs("swarm.key"); // Copy the DTP network swarm key

                    shell.ExecuteInline(ipfsexe, "init");
                    shell.ExecuteInline(ipfsexe, "bootstrap rm --all");
                    shell.ExecuteInline(ipfsexe, "bootstrap add /dns4/trust.dance/tcp/4001/ipfs/QmbQsznVuo4GA9qzWZXQVbonkw89uf4zyRp1iPoyef2dVo");
                }

                shell.ExecuteInline(ipfsexe, "daemon --enable-pubsub-experiment", null, Output);
            });
        }


        private void Output(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            if (data.Contains("Daemon is ready"))
                OnWaitForInputReady(new EventArgs());
        }

        protected virtual void OnWaitForInputReady(EventArgs e)
        {
            WaitForInputReady?.Invoke(this, e);
        }


        private void CopyToIpfs(string file)
        {
            File.Copy(Path.Combine(IpfsExePath, file), Path.Combine(IpfsDataPath, file));
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (shell != null)
                shell.Dispose();
            shell = null;

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            if (shell != null)
                shell.Dispose();

            base.Dispose();
        }
    }
}

