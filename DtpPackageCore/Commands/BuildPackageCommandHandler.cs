using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Notifications;
using DtpStampCore.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class BuildPackageCommandHandler :
        IRequestHandler<BuildPackageCommand, NotificationSegment>
    {
        private IMediator _mediator;
        private readonly IServerIdentityService _serverIdentityService;
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private IPackageService _trustPackageService;
        private NotificationSegment _notifications;
        private IConfiguration _configuration;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public BuildPackageCommandHandler(IMediator mediator, IServerIdentityService serverIdentityService, ITrustDBService trustDBService, IDerivationStrategyFactory derivationStrategyFactory, IPackageService trustPackageService, NotificationSegment notifications, IConfiguration configuration, ILogger<BuildPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _serverIdentityService = serverIdentityService;
            _trustDBService = trustDBService;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            _notifications = notifications;
            _configuration = configuration;
            this.logger = logger;
        }

        public async Task<NotificationSegment> Handle(BuildPackageCommand request, CancellationToken cancellationToken)
        {
            // Get current
            var buildPackages = await _trustDBService.GetBuildPackages();

            foreach (var buildPackage in buildPackages)
            {
                _trustDBService.LoadPackageClaims(buildPackage);

                if (buildPackage.Claims.Count == 0)
                {
                    _notifications.Add(new PackageNoClaimsNotification(buildPackage));
                    continue;
                }

                var builder = new PackageBuilder();
                builder.Package.Scopes = buildPackage.Scopes;

                _trustDBService.Add(builder.Package);

                foreach (var claim in buildPackage.Claims)
                {
                    if (claim.State.Match(ClaimStateType.Replaced))
                        _trustDBService.Remove(claim); // Should BuildPackageCommandHandler do the cleanup?
                    else
                    {
                        claim.ClaimPackages = claim.ClaimPackages.Where(p => p.PackageID != buildPackage.DatabaseID).ToList(); // Remove relation to static build package
                        claim.ClaimPackages.Add(new ClaimPackageRelationship { Claim = claim, Package = builder.Package }); // Add relation to new package

                        builder.AddClaim(claim);
                    }
                }

                if(builder.Package.Claims.Count == 0)
                {
                    _notifications.Add(new PackageNoClaimsNotification(buildPackage));
                    continue;
                }

                builder.OrderClaims(); // Order claims ny ID before package ID calculation. 
                SignPackage(builder);

                builder.Package.AddTimestamp(_mediator.SendAndWait(new CreateTimestampCommand { Source = builder.Package.Id }));

                _trustDBService.SaveChanges(); // Save the new package

                await _notifications.Publish(new PackageBuildNotification(builder.Package));
            }

            return _notifications;
        }

        private void SignPackage(PackageBuilder builder)
        {
            var serverSection = _configuration.GetModel(new ServerSection());
            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);

            builder.SetServer(scriptService.GetAddress(_serverIdentityService.Key));

            builder.Build();
            builder.Package.SetSignature(scriptService.SignMessage(_serverIdentityService.Key, builder.Package.Id));
            builder.Package.State = PackageStateType.Signed;
        }
    }
}
