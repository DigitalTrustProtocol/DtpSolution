using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DtpServerSpike.Commands
{
    [Command(Name = "id", Description = "Show info on DTP server")]
    public class IdCommand : CommandBase
    {
        [Argument(0, "id", "The DtpServer ID")]
        public string Id { get; set; }

        Program Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            var fileDestination = Path.Combine(Program.Platform.DtpServerDataPath, PlatformDirectory.ServerKeywordFilename);
            if (!File.Exists(fileDestination)) {
                app.Out.WriteLine($"Cannot find file {fileDestination}");
                return 1;
            }

            var id = await File.ReadAllTextAsync(fileDestination);
            app.Out.Write(id);
            app.Out.WriteLine();

            return 0;
        }

    }
}
