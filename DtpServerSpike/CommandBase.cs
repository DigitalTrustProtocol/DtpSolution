using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace DtpServerSpike
{
    [HelpOption("--help")]
    public abstract class CommandBase
    {
        protected virtual Task<int> OnExecute(CommandLineApplication app)
        {
            app.Error.WriteLine($"The command '{app.Name}' is not implemented.");
            return Task.FromResult(1);
        }
    }
}
