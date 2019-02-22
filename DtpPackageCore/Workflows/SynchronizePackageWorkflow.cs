using DtpCore.Workflows;
using MediatR;
using System;
using System.Linq;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DtpCore.Interfaces;
using System.ComponentModel;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Extensions;
using System.Collections.Generic;
using Ipfs;
using DtpCore.ViewModel;
using DtpCore.Model;
using DtpPackageCore.Commands;
using DtpPackageCore.Model;
using DtpCore.Collections.Generic;

namespace DtpPackageCore.Workflows
{
    /// <summary>
    /// Makes sure to synchronize packages with other servers.
    /// A very simple implementation, not efficient, needs a review and a better synchronization algo.
    /// When new nodes connect, they are asked for all of their packages information.
    /// Then on, new packages are discovered by the IPFS PubSub from the nodes.
    /// Reboot of the DtpServer, resets the workflow and all peers are requried.
    /// </summary>
    [DisplayName("Synchronize Packages")]
    [Description("Synchronize packages with other nodes.")]
    public class SynchronizePackageWorkflow : WorkflowContext
    {
        // A cache of peers last synchronized with.
        private static Dictionary<int, Peer> peerCache = new Dictionary<int, Peer>();
        private Dictionary<byte[], PackageInfo> packageCache = new Dictionary<byte[], PackageInfo>(ByteComparer.EqualityComparer);

        private IMediator _mediator;
        private readonly IPackageService _packageService;
        private ITrustDBService _trustDBService;
        private IPackageSchemaValidator _packageSchemaValidator;
        private ITimestampProofValidator _timestampProofValidator;
        private IConfiguration _configuration;
        private ILogger<SynchronizePackageWorkflow> _logger;

        public SynchronizePackageWorkflow(IMediator mediator, IPackageService packageService, ITrustDBService trustDBService, IPackageSchemaValidator packageSchemaValidator, ITimestampProofValidator timestampProofValidator, IConfiguration configuration, ILogger<SynchronizePackageWorkflow> logger)
        {
            _mediator = mediator;
            _packageService = packageService;
            _trustDBService = trustDBService;
            _packageSchemaValidator = packageSchemaValidator;
            _timestampProofValidator = timestampProofValidator;
            _configuration = configuration;
            _logger = logger;
        }

        // Last run of the synchronization
        public long LastSyncTime { get; set; }

        public override void Execute()
        {
            var scope = _configuration.PackageScope();

            var localPeer = _packageService.GetLocalPeer().GetAwaiter().GetResult();
            var localPeerHashId = localPeer.Id.GetHashCode();
            var peers = _packageService.GetPeersAsync(scope).GetAwaiter().GetResult();

            foreach (var peer in peers)
            {
                if (!peer.IsValid())
                    continue;

                var peerHashId = peer.Id.GetHashCode();
                if (peerHashId == localPeerHashId)
                    continue; // Do not process one self

                if (peerCache.ContainsKey(peerHashId))
                    continue; // Do not process known peers

                var cco = peer.ConnectedAddress;

                if(ProcessPeer(peer)) // All new peers gets processed
                    peerCache[peerHashId] = peer;
            }

            if (peers.Count() > 2) // Only save if where was more than 2 connections.
                LastSyncTime = DateTime.Now.ToUnixTime(); // Save last sync time.

            Wait(_configuration.SynchronizePackageWorkflowInterval()); // Never end the workflow
        }

        private bool ProcessPeer(Peer peer)
        {
            //_logger.LogInformation("Connected peer: " + peer.Id.ToString());
            if (peer.ConnectedAddress == null)
                return false;

            var ipV4 = peer.ConnectedAddress.Protocols.FirstOrDefault(p => "ip4".EndsWithIgnoreCase(p.Name));
            if (ipV4 == null)
                return false;

            var port = peer.ConnectedAddress.Protocols.FirstOrDefault(p => p.Code == 6);
            if (port == null || !"4001".EqualsIgnoreCase(port.Value)) // TODO: Do not call peers on other ports than 4001, they may be hidden behind a fireware without direct connection.
                return false;


            var ipAddress = ipV4.Value;
            // TODO: Should be optimized for better async handling.
            var info = _packageService.GetPackageInfoCollectionAsync(ipV4.Value, "", LastSyncTime).GetAwaiter().GetResult();
            if (info == null)
                return false;

            AddPackages(info);

            return true;
        }

        private void AddPackages(PackageInfoCollection infoCollection)
        {
            foreach (var packageInfo in infoCollection.Packages)
            {
                if (packageCache.ContainsKey(packageInfo.Id))
                    continue;

                if(_trustDBService.DoPackageExistAsync(packageInfo.Id).GetAwaiter().GetResult())
                    continue;

                // Get Package
                var package = _mediator.SendAndWait(new FetchPackageCommand(new PackageMessage { File = packageInfo.File }));
                if (ValidatePackage(packageInfo, package))
                {
                    // Now add the package to the system, the graph should automatically be updated as well.
                    var result = _mediator.Send(new AddPackageCommand(package)).GetAwaiter().GetResult();
                    packageCache[packageInfo.Id] = packageInfo;
                }

            }
        }

        private bool ValidatePackage(PackageInfo packageInfo, Package package)
        {
            if (package == null)
            {
                _logger.LogError($"Error wrong notitifiation returned from FatchPackageCommand");
                return false;
            }

            if (package.Timestamps == null || package.Timestamps.Count == 0)
            {
                _logger.LogError($"Error no timestamps was found on package id {package.Id}");
                return false;
            }

            // Verify package
            SchemaValidationResult validationResult = _packageSchemaValidator.Validate(package); // 
            if (validationResult.ErrorsFound > 0)
            {
                _logger.LogError(validationResult.ToString());
                return false;
            }

            if (!_timestampProofValidator.Validate(package.Timestamps[0], out IList<string> errors))
            {
                var msg = string.Join(", ", errors);
                _logger.LogError(msg);
                return false;
            }

            if(!ByteComparer.EqualityComparer.Equals(packageInfo.Id, package.Id))
            {
                _logger.LogError($"Error PackageInfo id {packageInfo.Id} is not the same as package id {package.Id} from file {packageInfo.File}");
                return false;
            }


            return true;
        }
    }

}
