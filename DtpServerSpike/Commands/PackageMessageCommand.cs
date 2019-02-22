using DtpCore.Services;
using DtpPackageCore.Model;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DtpServerSpike.Commands
{
    [Command(Name = "packagemessage", Description = "Send a package message")]
    public class PackageMessageCommand : CommandBase
    {
        [Argument(0, "packagemessage", "")]
        public string PackageMessage { get; set; }

        Program Parent { get; set; }



        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            var context = StartupSpike.CreateStartupSpike();

            if (string.IsNullOrEmpty(PackageMessage))
                PackageMessage = "ipfs://QmUWpEZyccMkPRhRL1wSYTZpcXDMNgHD5oEiF1i8XxCsbt";

            var message = new PackageMessage
            {
                File = PackageMessage,
                Scope = "twitter.com",
                ServerId = context.ServerIdentityService.Id
            };
            message.ServerSignature = context.ServerIdentityService.Sign(message.ToBinary());

            context.PackageService.PublishPackageMessageAsync(message);

            return Task.FromResult(0);
        }

    }
}
