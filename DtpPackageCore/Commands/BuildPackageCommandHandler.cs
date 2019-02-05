using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Model.Database;
using DtpCore.Notifications;
using DtpCore.Repository;
using DtpPackageCore.Exceptions;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Notifications;
using DtpStampCore.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private IPackageService _trustPackageService;
        private NotificationSegment _notifications;
        private IConfiguration _configuration;
        private readonly ILogger<BuildPackageCommandHandler> logger;
        private readonly IServiceProvider _serviceProvider;

        public BuildPackageCommandHandler(IMediator mediator, ITrustDBService trustDBService, IDerivationStrategyFactory derivationStrategyFactory, IPackageService trustPackageService, NotificationSegment notifications, IConfiguration configuration, ILogger<BuildPackageCommandHandler> logger, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            _notifications = notifications;
            _configuration = configuration;
            this.logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<NotificationSegment> Handle(BuildPackageCommand request, CancellationToken cancellationToken)
        {
            // Get current
            var buildPackage = _trustDBService.GetBuildPackage();

            _trustDBService.LoadPackageClaims(buildPackage);

            if(buildPackage.Claims.Count == 0)
            {
                _notifications.Add(new PackageNoClaimsNotification());
                return _notifications;
            }

            var builder = new PackageBuilder();
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

            //buildPackage.Claims = null;

            builder.OrderClaims(); // Order claims ny ID before package ID calculation. 
            SignPackage(builder);

            builder.Package.AddTimestamp(_mediator.SendAndWait(new CreateTimestampCommand { Source = builder.Package.Id }));

            _trustDBService.SaveChanges(); // Save the new package

            await _notifications.Publish(new PackageBuildNotification(builder.Package));

            return _notifications;
        }

        private void SignPackage(PackageBuilder builder)
        {
            var serverSection = _configuration.GetModel(new ServerSection());
            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);

            var key = scriptService.GetKey(Encoding.UTF8.GetBytes(serverSection.GetSecureKeyword()));

            builder.SetServer(scriptService.GetAddress(key));

            builder.Build();
            builder.Package.SetSignature(scriptService.SignMessage(key, builder.Package.Id));
            builder.Package.State = PackageStateType.Signed;
        }
    }
}
