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
using Topshelf;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace DtpServer
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {

            var isDevelopment = "Development".EqualsIgnoreCase(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            if(isDevelopment)
            {
                Console.WriteLine("Running in debug mode!");
                var service = new ServiceHandler();
                service.Start();
            }
            else
            {
                HostFactory.Run(windowsService =>
                {
                    windowsService.Service<ServiceHandler>(s =>
                    {
                        s.ConstructUsing(service => new ServiceHandler());
                        s.WhenStarted(service => service.Start());
                        s.WhenStopped(service => service.Stop());
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
            public static IConfiguration Configuration { get; private set; }

            public static PlatformDirectory Platform { get; private set; }


            public ServiceHandler()
            {
            }

            public void Start()
            {
                try
                {
                    Platform = new PlatformDirectory();
                    Platform.EnsureDtpServerDirectory();

                    SetupConfiguration();
                    SetupLogger();

                    Log.Information("Getting the motors running...");

                    // Renaming BuildWebHost to InitWebHost avoids problems with add-migration command.
                    // IDesignTimeDbContextFactory implemented for add-migration specifically.
                    InitWebHost(null).Run();

                    return;
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Host terminated unexpectedly");
                    return;
                }
                finally
                {
                    Log.Information("Started");
                    Log.CloseAndFlush();
                }
            }

            public void Stop()
            {
                Log.Information("Stopped");
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
                            Log.Information("Certiticate loaded!");
                        }
                        else
                        {
                            try
                            {
                                var cert = CertificateLoader.LoadFromStoreCert("localhost", "My", StoreLocation.CurrentUser, allowInvalid: true);
                                options.Listen(IPAddress.Loopback, 443, listenOptions =>
                                {
                                    listenOptions.UseHttps(cert);
                                });
                            }
                            catch
                            {
                                Log.Warning("Localhost https certificate not supported.");
                            }

                        }

                    })
                    .UseSerilog()
                    .Build();


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

            private static void SetupConfiguration()
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

            private static string EnsureDataFile(bool isDevelopment, string filename, string destination)
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
