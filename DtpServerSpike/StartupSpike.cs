﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DtpServer;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using DtpStampCore.Interfaces;
using UnitTest.DtpStampCore.Mocks;
using DtpCore.Interfaces;
using UnitTest.DtpPackage.Mocks;
using MediatR;
using DtpServer.Controllers;
using Ipfs.CoreApi;
using DtpPackageCore.Interfaces;
using DtpCore.Services;
using Serilog;
using Serilog.Events;
using UnitTest.DtpPackageCore.Mocks;

namespace DtpServerSpike
{
    public class StartupSpike : Startup, IDisposable
    {
        public IServiceProvider  ServiceProvider { get; set; }
        public IServiceScope ServiceScope { get; set; }
        public IServiceCollection Services { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public IMediator Mediator { get; set; }
        public TrustDBContext DB { get; set; }
        public ITrustDBService TrustDBService { get; set; }
        public IClaimBanListService ClaimBanListService { get; set; }
        public IPackageService PackageService { get; set; }
        public IServerIdentityService ServerIdentityService { get; set; }
        public IWorkflowService WorkflowService  { get; set; }

        //public static void AssemblyInitialize(TestContext context)
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        //        .MinimumLevel.Override("System", LogEventLevel.Warning)
        //        //.ReadFrom.Configuration(Configuration)
        //        .Enrich.FromLogContext()
        //        .WriteTo.Console()
        //        .CreateLogger();
        //}

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

            Services.AddTransient<PackageController>();
            Services.AddTransient<QueryController>();

            ServiceScope = Services.BuildServiceProvider(false).CreateScope();
            ServiceProvider = ServiceScope.ServiceProvider;

            PackageService = ServiceProvider.GetRequiredService<IPackageService>();
            ServerIdentityService = ServiceProvider.GetRequiredService<IServerIdentityService>();
            WorkflowService = ServiceProvider.GetRequiredService<IWorkflowService>();


            Mediator = ServiceProvider.GetRequiredService<IMediator>();
            TrustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            DB = TrustDBService.DBContext;

            ClaimBanListService = ServiceProvider.GetRequiredService<IClaimBanListService>();
        }

        public StartupSpike() : base(null, null)
        {
        }

        public StartupSpike(IConfiguration configuration) : base(null, configuration)
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

        public static StartupSpike CreateStartupSpike()
        {
            var startup = new StartupSpike(null);
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
