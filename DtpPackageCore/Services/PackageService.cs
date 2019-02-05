using DtpPackageCore.Configurations;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using DtpPackageCore.Notifications;
using Ipfs.Http;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace DtpPackageCore.Services
{
    public class PackageService : IPackageService
    {
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


        //private IMediator _mediator;
        public IpfsClient Ipfs { get; set; }
        public IConfiguration Configuration { get; set; }
        private readonly ILogger<PackageService> logger;
        private readonly IServiceProvider _serviceProvider;

        public PackageService(IpfsClient ipfs, IConfiguration configuration, ILogger<PackageService> logger, IServiceProvider serviceProvider)
        {
            //_mediator = mediator;
            Ipfs = ipfs;
            Configuration = configuration;
            this.logger = logger;
            _serviceProvider = serviceProvider;
        }

        public string CreatePackageName()
        {
            DateTime now = DateTime.Now;
            return $"Package_trustdance_{now.ToString("yyyyMMdd_hhmmss")}.json";
        }

        public async void AddPackageSubscriptions()
        {
            var config = PackageConfiguration.GetModel(Configuration);
            await AddPackageSubscriptions(config.ClaimTypes, config.ClaimScopes);
        }


        public async Task AddPackageSubscriptions(IEnumerable<string> claimTypes, IEnumerable<string> claimScopes)
        {
            var types = new List<string>(claimTypes);
            var scopes = new List<string>(claimScopes);

            if (types.Count == 0)
                types.Add(string.Empty);

            if (scopes.Count == 0)
                scopes.Add(string.Empty);


            foreach (var t in types)
            {
                foreach (var s in scopes)
                {
                    await Ipfs.PubSub.SubscribeAsync($"{t}{s}", this.HandleMessage, cancellationTokenSource.Token);
                }

            }

            await Ipfs.PubSub.SubscribeAsync("test", this.HandleMessage, cancellationTokenSource.Token);
        }

        public async void PublishPackageMessage(PackageMessage packageMessage)
        {
            var text = JsonConvert.SerializeObject(packageMessage, Formatting.None);
            await Ipfs.PubSub.PublishAsync("test", text);
        }



        private async void HandleMessage(Ipfs.IPublishedMessage publishedMessage)
        {
            try
            {
                //publishedMessage.Sender.Id.Digest

                using (var scope = _serviceProvider.CreateScope())
                {
                    var text = Encoding.UTF8.GetString(publishedMessage.DataBytes);
                    //var message = JsonConvert.DeserializeObject<PackageMessage>(text);

                    //var packageMessageValidator = scope.ServiceProvider.GetRequiredService<IPackageMessageValidator>();

                    logger.LogInformation(text);
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        // Check message syntaks
                        // Check against white/black lists of source.

                        // Call AddExternalPackageCommand

                    await mediator.Publish(new PackageMessageReceived(null));
                }
            }
            catch (Exception ex)
            {
                // Ignore errors  for now
                logger.LogError(ex.Message);
            }
        }

    }
}
