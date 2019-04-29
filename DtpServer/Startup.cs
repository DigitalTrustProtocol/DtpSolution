using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Extensions;
using DtpGraphCore.Extensions;
using DtpStampCore.Extensions;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using DtpServer.Middleware;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NicolasDorier.RateLimits;
using DtpServer.Extensions;
using Microsoft.Extensions.FileProviders;
using DtpPackageCore.Extensions;
using MediatR;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Serilog;
using System.Linq;
using DtpServer.Platform;
using DtpCore.Services;
using DtpCore.Service;
using DtpServer.Platform.ipfs;
using DtpPackageCore.Interfaces;
using System.Threading;

namespace DtpServer
{
    /// <summary>
    /// Startup class for Server
    /// </summary>
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;
        private IServiceCollection _services;
        private CancellationTokenSource cancellationTokenSource;
        public event EventHandler IPFSReady;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {

            _hostingEnv = env;
            Configuration = configuration;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (TimeMe.Track("ConfigureServices"))
            {
                //services.Configure<CookiePolicyOptions>(options =>
                //{
                //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //    options.CheckConsentNeeded = context => true;
                //    options.MinimumSameSitePolicy = SameSiteMode.None;
                //});

                ConfigureDbContext(services);

                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        //.AllowCredentials()
                        );
                });

                // Mvc stuff
                services.AddMvc(options =>
                {
                    if (!"Off".EndsWithIgnoreCase(Configuration.RateLimits()))
                        options.Filters.Add(new RateLimitsFilterAttribute("ALL") { Scope = RateLimitsScope.RemoteAddress });

                    options.Filters.Add(new RequestSizeLimitAttribute(10 * 1024 * 1024)); // 10Mb
                }).AddJsonOptions(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opts.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter
                    {
                        CamelCaseText = true
                    });
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

                services.AddHealthChecks()
                    .AddDbContextCheck<TrustDBContext>();

                services.AddRateLimits(); // Add Rate limits for against DDOS attacks

                services
                    .AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new Info
                        {
                            Version = "v1",
                            Title = "DTP API",
                            Description = "DTP API (ASP.NET Core 2.0)",
                            Contact = new Contact()
                            {
                                Name = "DTP",
                                Url = "https://trust.dance",
                                Email = ""
                            },
                            TermsOfService = "MIT"
                        });
                        c.CustomSchemaIds(type => type.FriendlyId(true));
                        c.DescribeAllEnumsAsStrings();
                        //if(_hostingEnv != null)
                        //    c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");

                        // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                        // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                        //c.OperationFilter<GeneratePathParamsValidationFilter>();
                    });



                services.AddMediatR();

                services.DtpCore();
                services.DtpGraphCore();
                services.DtpStrampCore();
                services.DtpPackageCore();
                services.DtpServer();

                AddBackgroundServices(services);

                _services = services;
            }
        }

        public virtual void ConfigureDbContext(IServiceCollection services)
        {
            using (TimeMe.Track("ConfigureDbContext"))
            {

                var platform = new PlatformDirectory();
                var dbName = "trust.db";
                var dbDestination = "./trust.db";
                if (_hostingEnv.IsProduction())
                {
                    dbDestination = Path.Combine(platform.DatabaseDataPath, dbName);
                    platform.EnsureDtpServerDirectory();
                    if (!File.Exists(dbDestination))
                        File.Copy(Path.Combine(dbName), dbDestination);
                }

                services.AddDbContext<TrustDBContext>(options =>
                    options.UseSqlite($"Filename={dbDestination}", b => b.MigrationsAssembly("DtpCore"))
                    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning))
                    );
            }
        }

        public virtual void AddBackgroundServices(IServiceCollection services)
        {
            using (TimeMe.Track("AddBackgroundServices"))
            {
                services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, IPFSShell>(serviceProvider => {
                    var ipfsShell = new IPFSShell(Program.Platform);
                    ipfsShell.WaitForInputReady += (sender, args) => OnIPFSReady(args);
                    return ipfsShell;
                    });

                services.AddScheduler((sender, args) =>
                {
#if DEBUG
                    Log.Error(args.Exception, args.Exception.Message);
#else
                    Log.Error(args.Exception.Message);
#endif
                    args.SetObserved();
                });
            }
        }

        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime, RateLimitService rateLimitService, ApplicationEvents applicationEvents)
        {
            app.AllServices(_services);
            applicationLifetime.ApplicationStopping.Register(() => applicationEvents.StopAsync().Wait());

            using (TimeMe.Track("IsDevelopment"))
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 100 });
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();

                    if (!"Off".EndsWithIgnoreCase(Configuration.RateLimits()))
                        rateLimitService.SetZone(Configuration.RateLimits());
                }

            }

            app.UseCors("CorsPolicy");

            using (TimeMe.Track("DTP apps"))
            {
                //app.StartIPFS();
                app.DtpCore(); // Ensure configuration of core
                app.DtpGraph(); // Load the Trust Graph from Database
                app.DtpStamp();

            }

            using (TimeMe.Track("Serilog, Swagger and UseMVC"))
            {

                app.UseMiddleware<SerilogDiagnostics>();
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                //app.UseCookiePolicy();
                //app.UseHealthChecks("/ready");

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                });

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                            name: "default",
                            template: "{controller}/{action=Index}/{id?}");
            });
            }

            IPFSReady += (sender, e) =>
            {
                // Wait for IPFS to be ready
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var packageService = scope.ServiceProvider.GetRequiredService<IPackageService>();
                    packageService.AddPackageSubscriptions();
                }
            };

            applicationEvents.WaitBootupTasksAsync().Wait();

        }

        protected virtual void OnIPFSReady(EventArgs e)
        {
            IPFSReady?.Invoke(this, e);
        }

    }
}
