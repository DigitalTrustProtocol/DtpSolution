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
        public const string FILE_SCHEME = "ipfs://";

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
            var pubSubController = _serviceProvider.GetRequiredService<PubSubController>();
            var ipfs = Ipfs;
            return Task.Run(() =>
            {
                try
                {
                    var list = new List<Task>();
                    if (!string.IsNullOrEmpty(scope))
                        list.Add(ipfs.PubSub.SubscribeAsync(scope, pubSubController.HandleMessage, cancellationTokenSource.Token));

                    list.Add(ipfs.PubSub.SubscribeAsync("global", pubSubController.HandleMessage, cancellationTokenSource.Token)); // Global scope (topic)

    #if DEBUG
                    list.Add(ipfs.PubSub.SubscribeAsync("test", pubSubController.HandleMessage, cancellationTokenSource.Token));

    #endif
                    Task.WaitAll(list.ToArray());
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            });
        }

        public async Task<Package> FetchPackageAsync(string path)
        {
            path = path.ReplaceIgnoreCase(FILE_SCHEME, ""); // Remove the "ipfs://" part

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
            return "ipfs://"+node.Id.Hash.ToString(); // Multihash value == id of file.
        }


        public async void PublishPackageMessageAsync(PackageMessage packageMessage)
        {
            var text = JsonConvert.SerializeObject(packageMessage, Formatting.None);
            await Ipfs.PubSub.PublishAsync(packageMessage.Scope, text);
            logger.LogInformation($"PackageMessage {packageMessage.File} has been published to network.");
        }


        public async Task<PackageInfoCollection> GetPackageInfoCollectionAsync(string ipAddress, string scope, long from)
        {
            var port = 80;
            var callUrl = new Uri($"http://{ipAddress}:{port}/api/package/info?from={from}");

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
