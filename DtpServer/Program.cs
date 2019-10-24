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
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using DtpServer.Platform;
using DtpCore.Extensions;
using Topshelf;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace DtpServer
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool isDevelopment = false;

        public static void Main(string[] args)
        {

            isDevelopment = "Development".EqualsIgnoreCase(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            if (isDevelopment)
            {
                Console.WriteLine("Running in debug mode!");
                var service = new ServiceHandler();
                service.Init().Run();
            }
            else
            {
                HostFactory.Run(windowsService =>
                {
                    windowsService.Service<ServiceHandler>(s =>
                    {
                        s.ConstructUsing(service => new ServiceHandler());
                        s.WhenStarted(service => service.Init().Start());
                        s.WhenStopped(service => service.Stop());
                        s.WhenShutdown(service => service.Stop());
                    });

                    windowsService.RunAsLocalSystem();
                    windowsService.StartAutomatically();

                    windowsService.SetDescription("DTP Server service for hosting trust");
                    windowsService.SetDisplayName("DTP Server");
                    windowsService.SetServiceName("DTPServer");
                });

            }
        }


        class ServiceHandler
        {
            /// <summary>
            /// Static configuration 
            /// </summary>
            public IConfiguration Configuration { get; private set; }

            public PlatformDirectory Platform { get; private set; }

            private IHost webHost;

            public ServiceHandler()
            {
            }

            public IHost Init()
            {
                Platform = new PlatformDirectory();
                Platform.EnsureDtpServerDirectory();

                SetupConfiguration();
                SetupLogger();

                Log.Information("Getting the motors running...");
                    
                // Renaming BuildWebHost to InitWebHost avoids problems with add-migration command.
                // IDesignTimeDbContextFactory implemented for add-migration specifically.
                webHost = CreateHostBuilder(null).Build();
                return webHost;
            }

            public void Stop()
            {
                webHost?.Dispose();
                Log.Information("Stopped");
                Log.CloseAndFlush();
            }


            public IHostBuilder CreateHostBuilder(string[] args) =>
                Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        //webBuilder.UseUrls("http://trust.dance");
                        webBuilder.UseConfiguration(Configuration);
                        webBuilder.UseDefaultServiceProvider((context, options) =>
                                {
                                    options.ValidateScopes = isDevelopment;// context.HostingEnvironment.IsDevelopment();
                                });
                        //webBuilder.CaptureStartupErrors(true);
                        webBuilder.UseSerilog();
                    });



            private void SetupLogger()
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

            private void SetupConfiguration()
            {
                var serverKeywordFilename = "ServerKeyword.json";
                var isDevelopment = "Development".EqualsIgnoreCase(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                var configfile = EnsureDataFile(isDevelopment, "appsettings.json", Platform.DtpServerDataPath); // GetPfxFile
                var configKeyword = EnsureDataFile(isDevelopment, serverKeywordFilename, Platform.DtpServerDataPath); // Keyword for signing the packages

                Configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configfile, optional: false, reloadOnChange: true)
                .AddJsonFile(configKeyword, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                if (!isDevelopment && string.IsNullOrEmpty(Configuration.GetValue<string>("ServerKeyword")))
                {
                    var keyword = Guid.NewGuid().ToByteArray().ToHex();
                    var json = $"{{ \"ServerKeyword\": \"{keyword}\" }}";
                    File.WriteAllText(Path.Combine(Platform.DtpServerDataPath, serverKeywordFilename), json);
                }
            }

            private string EnsureDataFile(bool isDevelopment, string filename, string destination)
            {
                if (isDevelopment)
                    return filename;

                if (!Directory.Exists(destination))
                {
                    Log.Information("Creating destination: " + destination);
                    Directory.CreateDirectory(destination);
                }


                var fileDestination = Path.Combine(destination, filename);
                if (!File.Exists(fileDestination))
                {
                    if (!File.Exists(filename))
                        return "";

                    File.Copy(filename, fileDestination);
                }

                return fileDestination;
            }
        }



    }
}
