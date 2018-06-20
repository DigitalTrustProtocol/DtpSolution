using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    public class ControllerMock
    {
        public WebHostBuilder HostBuilder { get; set; }
        public TestServer Server { get; set; }

        [TestInitialize]
        public void Init()
        {
            HostBuilder = new WebHostBuilder();
            HostBuilder.UseStartup<StartupMock>();
            Server = new TestServer(HostBuilder);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Server != null)
                Server.Dispose();
        }

    }
}
