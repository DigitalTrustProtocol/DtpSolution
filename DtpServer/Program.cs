using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using Serilog;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;
using Serilog.Events;
using Microsoft.AspNetCore.Server.Kestrel.Https.Internal;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace DtpServer
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Static configuration 
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static int Main(string[] args)
        {
            var formatter = (false) ? (ITextFormatter)new RenderedCompactJsonFormatter() : new MessageTemplateTextFormatter("{Timestamp:o} {RequestId,13} [{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}", null);

            var pathFormat = "Logs/log-{Date}.txt";
            const long DefaultFileSizeLimitBytes = 1024 * 1024 * 1024;
            const int DefaultRetainedFileCountLimit = 31;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Async(w => w.RollingFile(
                    formatter,
                    Environment.ExpandEnvironmentVariables(pathFormat),
                    fileSizeLimitBytes: DefaultFileSizeLimitBytes,
                    retainedFileCountLimit: DefaultRetainedFileCountLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(2)))
                .CreateLogger();

            try
            {
                Log.Information("Getting the motors running...");

                // Renaming BuildWebHost to InitWebHost avoids problems with add-migration command.
                // IDesignTimeDbContextFactory implemented for add-migration specifically.
                InitWebHost(args).Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
            
        }

        public static IWebHost InitWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(Configuration)
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10Mb, 

                    //var file = "/root/.aspnet/https/" + "trust.dance.pfx";
                    var file = "trust.dance.pfx";
                    options.Listen(IPAddress.Any, 80);

                    if (File.Exists(file))
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(file, "123");
                        });
                    }
                    else
                    {
                        options.Listen(IPAddress.Loopback, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(CertificateLoader.LoadFromStoreCert("localhost", "My", StoreLocation.CurrentUser, allowInvalid: true));
                        });
                    }

                })
                .UseSerilog()
                .Build();
    }
}
