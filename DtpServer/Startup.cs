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

namespace DtpServer
{
    /// <summary>
    /// Startup class for Server
    /// </summary>
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;
        private IServiceCollection _services;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _hostingEnv = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            ConfigureDbContext(services);
            
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

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = false;
            });

            services.AddDirectoryBrowser();

            _services = services;
        }

        public virtual void ConfigureDbContext(IServiceCollection services)
        {
            services.AddDbContext<TrustDBContext>(options =>
                options.UseSqlite("Filename=./trust.db", b => b.MigrationsAssembly("DtpCore"))
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning))
                );
        }

        public virtual void AddBackgroundServices(IServiceCollection services)
        {
            services.AddScheduler((sender, args) =>
            {
                Console.Write(args.Exception.Message);
                args.SetObserved();
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="rateLimitService"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, RateLimitService rateLimitService)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //loggerFactory.AddConsole();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();

                if (!"Off".EndsWithIgnoreCase(Configuration.RateLimits()))
                    rateLimitService.SetZone(Configuration.RateLimits());
            }


            app.DtpCore(); // Ensure configuration of core
            app.DtpGraph(); // Load the Trust Graph from Database
            app.DtpStamp();
            app.DtpPackage();

            app.UseMiddleware<SerilogDiagnostics>();
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            // public file folder
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(PublicFileRepository.PublicFullPath),
                RequestPath = PublicFileRepository.REQUESTPATH,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age=31536000"); // 1 year
                }
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(PublicFileRepository.PublicFullPath),
                RequestPath = PublicFileRepository.REQUESTPATH,
            });


            app.UseCookiePolicy();
            app.UseHealthChecks("/ready");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });

            app.UseSession();


            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //    name: "stamp",
                //    template: "v1/stamp/{blockchain}/{action=Index}/{source?}");
                //routes.MapRoute("Proof.htm");

                routes.MapRoute(
                    name: "default",
                    template: "v1/{controller}/{action=Index}/{id?}");
            });


            app.AllServices(_services);
        }

    }
}
