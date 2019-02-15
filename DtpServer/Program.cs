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
using DtpServer.Platform;
using DtpCore.Extensions;

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
        public static IConfiguration Configuration { get; private set; } 

        public static PlatformDirectory Platform { get; private set; }

        public static int Main(string[] args)
        {
            Platform = new PlatformDirectory();
            Platform.EnsureDtpServerDirectory();

            SetupConfiguration();
            SetupLogger();

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
                .UseKestrel((context, options) =>
                {
                    var isDevelopment = context.HostingEnvironment.IsDevelopment();

                    options.AddServerHeader = false;
                    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10Mb, 

                    options.Listen(IPAddress.Any, 80);

                    var file = EnsureDataFile(isDevelopment, "domaincert.pfx", Platform.DtpServerDataPath); // GetPfxFile
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

        private static void SetupConfiguration()
        {
            var serverKeywordFilename = "ServerKeyword.json";
            var isDevelopment = "Development".EqualsIgnoreCase(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            var configfile = EnsureDataFile(isDevelopment, "appsettings.json", Platform.DtpServerDataPath); // GetPfxFile
            var configKeyword = EnsureDataFile(isDevelopment, serverKeywordFilename, Platform.DtpServerDataPath); // Keyword for signing the packages

            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configfile, optional: false, reloadOnChange: true)
            .AddJsonFile(configKeyword, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            if(!isDevelopment && string.IsNullOrEmpty(Configuration.GetValue<string>("ServerKeyword")))
            {
                var keyword = Guid.NewGuid().ToByteArray().ToHex();
                var json = $"{{ \"ServerKeyword\": \"{keyword}\" }}";
                File.WriteAllText(Path.Combine(Platform.DtpServerDataPath, serverKeywordFilename), json);
            }
        }

        private static void SetupLogger()
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
        }

        private static string EnsureDataFile(bool isDevelopment, string filename, string destination)
        {
            if (isDevelopment)
                return filename;

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            var fileDestination = Path.Combine(destination, filename);
            if (!File.Exists(fileDestination))
                File.Copy(filename, fileDestination);

            return fileDestination;
        }

    }
}
