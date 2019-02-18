using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
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
using DtpCore.Model;
using DtpPackageCore.Extensions;
using Ipfs.CoreApi;
using DtpPackageCore.Commands;
using Ipfs;
using DtpCore.Interfaces;
using DtpCore.Extensions;
using DtpCore.ViewModel;
using System.Net;

namespace DtpPackageCore.Services
{
    public class IpfsPackageService : IPackageService
    {
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ICoreApi Ipfs { get; set; }
        public IConfiguration Configuration { get; set; }
        private readonly IServiceProvider _serviceProvider;
        private IServerIdentityService _serverIdentityService;
        private readonly ILogger<IpfsPackageService> logger;


        public IpfsPackageService(ICoreApi ipfs, IConfiguration configuration, IServiceProvider serviceProvider, IServerIdentityService serverIdentityService, ILogger<IpfsPackageService> logger)
        {
            Ipfs = ipfs;
            Configuration = configuration;
            _serviceProvider = serviceProvider;
            _serverIdentityService = serverIdentityService;
            this.logger = logger;
        }


        public Task<Peer> GetLocalPeer()
        {
            return Ipfs.Generic.IdAsync();
        }


        public async Task<IEnumerable<Peer>> GetPeersAsync(string scope)
        {
            var list = new List<Peer>();
            try
            {
                list.AddRange(await Ipfs.Swarm.PeersAsync());
            }
            catch
            {
                logger.LogWarning("No peers");
            }

            return list;
        }


        public Task AddPackageSubscriptionsAsync()
        {
            return AddPackageSubscriptionsAsync(Configuration.PackageScope());
        }

        public Task AddPackageSubscriptionsAsync(string scope)
        {
            return Task.Run(() =>
            {
                var list = new List<Task>();
                if (!string.IsNullOrEmpty(scope))
                    list.Add(Ipfs.PubSub.SubscribeAsync(scope, this.HandleMessage, cancellationTokenSource.Token));

                list.Add(Ipfs.PubSub.SubscribeAsync("", this.HandleMessage, cancellationTokenSource.Token)); // Global scope (topic)

#if DEBUG
                list.Add(Ipfs.PubSub.SubscribeAsync("test", this.HandleMessage, cancellationTokenSource.Token));

#endif
                Task.WaitAll(list.ToArray());
            });
        }

        public async Task<Package> FetchPackageAsync(string path)
        {
            var json = await Ipfs.FileSystem.ReadAllTextAsync(path); // Load the file from the IPFS network!
            var package = JsonConvert.DeserializeObject<Package>(json);
            package.File = path;
            return package;
        }

        public async Task<string> StorePackageAsync(Package package)
        {
            var content = JsonConvert.SerializeObject(package, Formatting.None);
            var node = await Ipfs.FileSystem.AddTextAsync(content) as FileSystemNode;
            logger.LogInformation($"Package {package.DatabaseID} has been stored on drive.");
            return node.Id.Hash.ToString(); // Multihash value == id of file.
        }


        public async void PublishPackageMessageAsync(PackageMessage packageMessage)
        {
            var text = JsonConvert.SerializeObject(packageMessage, Formatting.None);
            await Ipfs.PubSub.PublishAsync(packageMessage.Scope, text);
            logger.LogInformation($"PackageMessage {packageMessage.File} has been published to network.");
        }


        /// <summary>
        /// Only raw processing, no check or validation of Peers. 
        /// </summary>
        /// <param name="publishedMessage"></param>
        private async void HandleMessage(IPublishedMessage publishedMessage)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Create a scope so all dependency injections are available.
                    var text = Encoding.UTF8.GetString(publishedMessage.DataBytes);

                    var packageMessage = JsonConvert.DeserializeObject<PackageMessage>(text);
                    if (_serverIdentityService.Id.Equals(packageMessage.ServerId)) // Do not process own package messages
                        return;

                    logger.LogInformation($"Received message from {packageMessage.ServerId}");


                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var package = mediator.SendAndWait(new FetchPackageCommand(packageMessage));

                    if (package == null)
                    {
                        logger.LogError($"Error wrong notitifiation returned from FatchPackageCommand");
                        return;
                    }

                    if (package.Timestamps == null || package.Timestamps.Count == 0)
                    {
                        logger.LogError($"Error no timestamps was found on package id {package.Id} from server id {packageMessage.ServerId}");
                        return;
                    }

                    var packageValidator = scope.ServiceProvider.GetRequiredService<IPackageSchemaValidator>();
                    SchemaValidationResult validationResult = packageValidator.Validate(package); // 
                    if(validationResult.ErrorsFound > 0)
                    {
                        logger.LogError(validationResult.ToString());
                        return;
                    }

                    var timestampValidator = scope.ServiceProvider.GetRequiredService<ITimestampProofValidator>();
                    if(!timestampValidator.Validate(package.Timestamps[0], out IList<string> errors))
                    {
                        var msg = string.Join(", ", errors);
                        logger.LogError(msg);
                        return;
                    }

                    //var peers = await Ipfs.PubSub.PublishAsync;

                    // Now add the package!
                    await mediator.Send(new AddPackageCommand(package));
                }
            }
            catch (Exception ex)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var msg = $"Received message from server {publishedMessage.Id} failed with: {ex.Message}";
                    var log = scope.ServiceProvider.GetRequiredService<ILogger<IpfsPackageService>>();
                    log.LogError(msg);
                }
                // No logging possiblilities here!

                //LogInfo(msg).Wait();
                //Console.WriteLine(msg);
                //var log = scope.ServiceProvider.GetRequiredService<ILogger<IpfsPackageService>>();
                //log.LogError(msg);
            }
        }

        public async Task<PackageInfoCollection> GetPackageInfoCollectionAsync(string ipAddress, string scope, long from)
        {
            var port = 80;
            var callUrl = new Uri($"http://{ipAddress}:{port}/api/packages/info?from={from}");

            try
            {

                using (var client = new WebClient())
                {
                    // Get packages from server

                    var json = await client.DownloadStringTaskAsync(callUrl);

                    var packageInfoCollection = JsonConvert.DeserializeObject<PackageInfoCollection>(json);

                    return packageInfoCollection;

                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get packages from server {ipAddress} - Error : {ex.Message}");
                return null;
            }
        }
    }
}
