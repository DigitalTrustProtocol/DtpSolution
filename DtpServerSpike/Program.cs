using DtpCore.Extensions;
using DtpServerSpike.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DtpServerSpike
{
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(PackageMessageCommand))]
    [Subcommand(typeof(IdCommand))]
    [Subcommand(typeof(ClaimProducerCommand))]
    public class Program : CommandBase
    {
        public static IConfiguration Configuration { get; private set; }

        public static PlatformDirectory Platform { get; private set; }

        [Option("--enc", Description = "The output type (json, xml, or text)")]
        public string OutputEncoding { get; set; } = "text";

        //[Option("--debug", Description = "Show debugging info")]
        //public bool Debug { get; set; }  // just for documentation, already parsed in Main

        //[Option("--trace", Description = "Show tracing info")]
        //public bool Trace { get; set; }  // just for documentation, already parsed in Main

        [Option("--time", Description = "Show how long the command took")]
        public bool ShowTime { get; set; }


        static int Main(string[] args)
        {
            var startTime = DateTime.Now;
            Platform = new PlatformDirectory();
            Platform.EnsureDtpServerDirectory();

            SetupConfiguration();
            SetupLogger();

            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //for (; e != null; e = e.InnerException)
                //{
                //    Console.Error.WriteLine(e.Message);
                //    if (debugging || tracing)
                //    {
                //        Console.WriteLine();
                //        Console.WriteLine(e.StackTrace);
                //    }
                //}
                return 1;
            }

            var took = DateTime.Now - startTime;
            Console.Write($"Took {took.TotalSeconds} seconds.");
            Console.ReadKey();

            return 0;
        }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(0);
        }


        private static void SetupConfiguration()
        {
            //var isDevelopment = "Development".EqualsIgnoreCase(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            //Configuration = new ConfigurationBuilder()
            //.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            ////.AddEnvironmentVariables()
            //.Build();
        }

        private static void SetupLogger()
        {
            var formatter = (false) ? (ITextFormatter)new RenderedCompactJsonFormatter() : new MessageTemplateTextFormatter("{Timestamp:o} {RequestId,13} [{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}", null);

            //var pathFormat = "Logs/log-{Date}.txt";
            //const long DefaultFileSizeLimitBytes = 1024 * 1024 * 1024;
            //const int DefaultRetainedFileCountLimit = 31;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                //.ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                //.WriteTo.Async(w => w.RollingFile(
                //    formatter,
                //    Environment.ExpandEnvironmentVariables(pathFormat),
                //    fileSizeLimitBytes: DefaultFileSizeLimitBytes,
                //    retainedFileCountLimit: DefaultRetainedFileCountLimit,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(2)))
                .CreateLogger();


        }

        public int Output<T>(CommandLineApplication app, T data, Action<T, TextWriter> text) where T : class
        {
            if (text == null)
            {
                OutputEncoding = "json";
            }

            switch (OutputEncoding.ToLowerInvariant())
            {
                case "text":
                    text(data, app.Out);
                    break;

                case "json":
                    var x = new JsonSerializer();
                    x.Formatting = Formatting.Indented;
                    x.Serialize(app.Out, data);
                    break;

                default:
                    app.Error.WriteLine($"Unknown output encoding '{OutputEncoding}'");
                    return 1;
            }

            return 0;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

    }



}
