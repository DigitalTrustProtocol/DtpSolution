using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using DtpCore.Repository;
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
        IRequestHandler<BuildPackageCommand, Package>
    {
        private IMediator _mediator;
        private readonly IServerIdentityService _serverIdentityService;
        private readonly TrustDBContext _db;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private IPackageService _trustPackageService;
        private NotificationSegment _notifications;
        private IConfiguration _configuration;
        private readonly ILogger<BuildPackageCommandHandler> logger;

        public BuildPackageCommandHandler(IMediator mediator, IServerIdentityService serverIdentityService, TrustDBContext db, IDerivationStrategyFactory derivationStrategyFactory, IPackageService trustPackageService, NotificationSegment notifications, IConfiguration configuration, ILogger<BuildPackageCommandHandler> logger)
        {
            _mediator = mediator;
            _serverIdentityService = serverIdentityService;
            _db = db;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            _notifications = notifications;
            _configuration = configuration;
            this.logger = logger;
        }

        public async Task<Package> Handle(BuildPackageCommand request, CancellationToken cancellationToken)
        {
            var sourcePackage = request.SourcePackage;
            
            // Ensure to load the claims
            if(sourcePackage.Claims.Count == 0 || sourcePackage.ClaimPackages.Count == 0)
                _db.LoadPackageClaims(sourcePackage);

            if (sourcePackage.Claims.Count == 0)
            {
                _notifications.Add(new PackageNoClaimsNotification(sourcePackage));
                return null;
            }

            // Build a new Package 
            var builder = new PackageBuilder();
            builder.Package.Scopes = sourcePackage.Scopes;
            _db.Packages.Add(builder.Package); // Add the package to the DB context, so it will be handle in the claims bindings.

            foreach (var claim in sourcePackage.Claims)
            {
                if (claim.State.Match(ClaimStateType.Replaced))
                    _db.Claims.Remove(claim); // Should BuildPackageCommandHandler do the cleanup?
                else
                {
                    claim.ClaimPackages = claim.ClaimPackages.Where(p => p.PackageID != sourcePackage.DatabaseID).ToList(); // Remove relation to static build package
                    claim.ClaimPackages.Add(new ClaimPackageRelationship { Claim = claim, Package = builder.Package }); // Add relation to new package

                    builder.AddClaim(claim);
                }
            }

            if (builder.Package.Claims.Count == 0)
            {
                _notifications.Add(new PackageNoClaimsNotification(sourcePackage));
                return null;
            }

            builder.OrderClaims(); // Order claims now ID before package ID calculation. 
            SignPackage(builder);

            // Add timestamp to package, DB will auto connect from object reference
            builder.Package.AddTimestamp(await _mediator.Send(new CreateTimestampCommand(builder.Package.Id)));

            // Save the new package before calling notifications
            await _db.SaveChangesAsync(); 

            await _notifications.Publish(new PackageBuildNotification(builder.Package));

            return builder.Package;
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
