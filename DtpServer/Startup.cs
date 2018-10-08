using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
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

namespace DtpServer
{
    public class Startup
    {
        private IServiceCollection _services;

        public Startup(IConfiguration configuration)
        {
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
                options.Filters.Add(new RateLimitsFilterAttribute("ALL") { Scope = RateLimitsScope.RemoteAddress });
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddRateLimits(); // Add Rate limits for against DDOS attacks

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "DTP API",
                    Version = "v1",
                });
            });

            services.DtpCore();
            services.DtpGraphCore();
            services.DtpStrampCore();

            AddBackgroundServices(services);

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = false;
            });

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                app.UseHsts();
                rateLimitService.SetZone($"zone=ALL rate=10r/s");
            }

            app.Graph(); // Load the Trust Graph from Database
            app.Truststamp();

            app.UseMiddleware<SerilogDiagnostics>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
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
