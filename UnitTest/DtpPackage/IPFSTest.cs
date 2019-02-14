using Common.Logging;
using Ipfs.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTest.DtpPackage
{
    [TestClass]
    public class IPFSTest
    {
        const string passphrase = "this is not a secure pass phrase";
        public static IpfsEngine Ipfs = new IpfsEngine(passphrase.ToCharArray());
        public static IpfsEngine IpfsOther = new IpfsEngine(passphrase.ToCharArray());

        static IPFSTest()
        {
            Ipfs.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), "ipfs-test");
            Ipfs.Options.KeyChain.DefaultKeySize = 512;
            Ipfs.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/4001" })
            ).Wait();

            IpfsOther.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), "ipfs-other");
            IpfsOther.Options.KeyChain.DefaultKeySize = 512;
            IpfsOther.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/0" })
            ).Wait();
        }

        [TestMethod]
        public void Engine_Exists()
        {
            Assert.IsNotNull(Ipfs);
            Assert.IsNotNull(IpfsOther);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            if (Directory.Exists(Ipfs.Options.Repository.Folder))
            {
                Directory.Delete(Ipfs.Options.Repository.Folder, true);
            }
            if (Directory.Exists(IpfsOther.Options.Repository.Folder))
            {
                Directory.Delete(IpfsOther.Options.Repository.Folder, true);
            }
        }
    }
}
