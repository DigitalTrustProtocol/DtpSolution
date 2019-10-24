using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Extensions;
using DtpGraphCore.Extensions;
using DtpStampCore.Extensions;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using DtpServer.Middleware;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NicolasDorier.RateLimits;
using DtpServer.Extensions;
using DtpPackageCore.Extensions;
using MediatR;
using System.IO;
using Serilog;
using DtpCore.Service;
using DtpServer.Platform;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Json.Serialization;

namespace DtpServer
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private IServiceCollection _services;

        public IConfiguration Configuration { get; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {

            _hostingEnv = env;
            Configuration = configuration;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (TimeMe.Track("ConfigureServices"))
            {
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

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

                services.AddControllers().AddJsonOptions(options => {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                });
                services.AddRazorPages(config =>
                {
                });

                //services.AddMvc()
                //    .AddJsonOptions(options => {
                //        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                //        options.JsonSerializerOptions.IgnoreNullValues = true;
                //    });
                ////                services.AddMvc(options =>
                //                {
                //                    //if (!"Off".EndsWithIgnoreCase(Configuration.RateLimits()))
                //                    //    options.Filters.Add(new RateLimitsFilterAttribute("ALL") { Scope = RateLimitsScope.RemoteAddress });

                //                    //options.Filters.Add(new RequestSizeLimitAttribute(10 * 1024 * 1024)); // 10Mb
                //                    options.EnableEndpointRouting = false;
                //                });
                //                .AddNewtonsoftJson(opts =>
                //                //{
                //                //    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //                //    opts.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter
                //                //    {
                //                //        CamelCaseText = true
                //                //    });
                //                //}); //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

                services.AddHealthChecks()
                    .AddDbContextCheck<TrustDBContext>();

                services.AddRateLimits(); // Add Rate limits for against DDOS attacks

                //services.AddSwaggerGen(c =>
                //{
                //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                //});

                services
                    .AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo
                        {
                            Version = "v1",
                            Title = "DTP API",
                            Description = "DTP API (ASP.NET Core 3.0)",
                            Contact = new OpenApiContact()
                            {
                                Name = "DTP",
                                Url = new Uri("https://trust.dance"),
                                Email = ""
                            },
                            TermsOfService = new Uri("https://raw.githubusercontent.com/DigitalTrustProtocol/DtpSolution/master/License")
                        });
                        //c.CustomSchemaIds(type => type..FriendlyId(true));
                        //c.DescribeAllEnumsAsStrings();
                        //if(_hostingEnv != null)
                        //    c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");

                        // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                        // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                        //c.OperationFilter<GeneratePathParamsValidationFilter>();
                    });

                services.AddMediatR(Assembly.GetExecutingAssembly());

                services.DtpCore();
                services.DtpGraphCore();
                services.DtpStrampCore();
                services.DtpPackageCore();
                services.DtpServer();

                AddBackgroundServices(services);

                _services = services;

            }

        }

        //        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //            //app.AllServices(_services);
            //            //applicationLifetime.ApplicationStopping.Register(() => applicationEvents.StopAsync().Wait());


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 100 });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                //                    //if (!"Off".EndsWithIgnoreCase(Configuration.RateLimits()))
                //                    //    rateLimitService.SetZone(Configuration.RateLimits());
                

                app.UseCors("CorsPolicy");
            }

            using (TimeMe.Track("DTP apps"))
            {
                //app.DtpCore(); // Ensure configuration of core
                app.DtpGraph(); // Load the Trust Graph from Database
                app.DtpStamp();
            }

            using (TimeMe.Track("Serilog, Swagger and UseMVC"))
            {

                app.UseMiddleware<SerilogDiagnostics>();
                //app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();
                //app.UseAuthorization();

                //app.UseFileServer(enableDirectoryBrowsing: true);
                //app.UseCookiePolicy();
                app.UseHealthChecks("/ready");

                //Enable middleware to serve generated Swagger as a JSON endpoint.
                // Enable middleware to serve swagger - ui(HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                });


                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapRazorPages();
                });
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
    }
}
