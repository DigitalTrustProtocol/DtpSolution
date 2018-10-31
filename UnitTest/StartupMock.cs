using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DtpServer;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using DtpStampCore.Interfaces;
using UnitTest.DtpStampCore.Mocks;
using DtpGraphCore.Controllers;
using DtpCore.Interfaces;
using UnitTest.DtpPackage.Mocks;
using MediatR;

namespace UnitTest
{
    public class StartupMock : Startup, IDisposable
    {
        public IServiceProvider  ServiceProvider { get; set; }
        public IServiceScope ServiceScope { get; set; }
        public IServiceCollection Services { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public IMediator Mediator { get; set; }

        [TestInitialize]
        public virtual void Init()
        {
            Services = new ServiceCollection();
            ConfigureServices(Services);

            Services.AddTransient<IConfiguration>(p => {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json.config", optional: true).Build();
                config["blockchain"] = "btctest"; // Use bitcoin test net
                config["btctest_fundingkey"] = "cMcGZkth7ufvQC59NSTSCTpepSxXbig9JfhCYJtn9RppU4DXx4cy"; // btc test net WIF key 
                return config;
                });

            //Services.AddTransient<IBlockchainRepository, BlockchainRepositoryMock>();
            Services.AddTransient<IPublicFileRepository, PublicFileRepositoryMock>();

            Services.AddTransient<TrustController>();
            Services.AddTransient<QueryController>();

            ServiceScope = Services.BuildServiceProvider(false).CreateScope();
            ServiceProvider = ServiceScope.ServiceProvider;
            LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            LoggerFactory.AddConsole();

            Mediator = ServiceProvider.GetRequiredService<IMediator>();

        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            ServiceScope.Dispose();
        }

        public StartupMock() : base(null)
        {
        }

        public StartupMock(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureDbContext(IServiceCollection services)
        {
            services.AddDbContext<TrustDBContext>(options =>
                options.UseInMemoryDatabase("UnitTest")
                
            );
        }

        public override void AddBackgroundServices(IServiceCollection services)
        {
            // Do not create timers here!
        }

        public static StartupMock CreateStartupTest()
        {
            var startup = new StartupMock(null);
            startup.Init();
            return startup;
        }

        public static TrustDBContext CreateDBContext()
        {
            var options = new DbContextOptionsBuilder<TrustDBContext>()
                    .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                    .Options;

            // Run the test against one instance of the context
            return new TrustDBContext(options);
        }


        public void Dispose()
        {
            if (ServiceScope != null)
                ServiceScope.Dispose();
        }
    }
}
