using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace IS4Amin.STS.Identity
{


    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
	            .UseSerilog()
				.Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseUrls("http://sts.trust.dance")
                //.UseKestrel()
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>();

    }
}
