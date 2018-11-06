using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
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
    public class BuildTrustPackageCommandHandler :
        IRequestHandler<BuildTrustPackageCommand, NotificationSegment>
    {
        private IMediator _mediator;

        private TrustDBContext _dbContext;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private ITrustPackageService _trustPackageService;
        private NotificationSegment _notifications;
        private IConfiguration _configuration;
        private readonly ILogger<BuildTrustPackageCommandHandler> logger;
        private readonly IServiceProvider _serviceProvider;

        public BuildTrustPackageCommandHandler(IMediator mediator, TrustDBContext db, IDerivationStrategyFactory derivationStrategyFactory, ITrustPackageService trustPackageService, NotificationSegment notifications, IConfiguration configuration, ILogger<BuildTrustPackageCommandHandler> logger, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _dbContext = db;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            _notifications = notifications;
            _configuration = configuration;
            this.logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<NotificationSegment> Handle(BuildTrustPackageCommand request, CancellationToken cancellationToken)
        {
            var trusts = GetTrusts();
            TrustBuilder _builder = new TrustBuilder(_serviceProvider); // _serviceProvider.GetRequiredService<TrustBuilder>();
            _builder.AddTrust(trusts);
            _builder.OrderTrust(); // Order trust ny ID before package ID calculation. 
            if (_builder.Package.Trusts.Count == 0)
            {
                // No trusts found, exit
                _notifications.Add(new TrustPackageNoTrustNotification());
                return _notifications;
            }

            SignPackage(_builder);

            _builder.Package.AddTimestamp(_mediator.SendAndWait(new CreateTimestampCommand { Source = _builder.Package.Id }));

            _dbContext.Packages.Add(_builder.Package);
            _dbContext.SaveChanges();

            await _notifications.Publish(new TrustPackageBuildNotification(_builder.Package));

            return _notifications;
        }

        private void SignPackage(TrustBuilder builder)
        {
            var serverSection = _configuration.GetModel(new ServerSection());
            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);

            var key = scriptService.GetKey(Encoding.UTF8.GetBytes(serverSection.GetSecureKeyword()));

            builder.SetServer(scriptService.GetAddress(key));

            builder.Build();
            builder.Package.SetSignature(scriptService.SignMessage(key, builder.Package.Id));
        }

        private IQueryable<Trust> GetTrusts()
        {
            // Get all trusts from LastTrustDatabaseID to now
            var trusts = from trust in _dbContext.Trusts.Include(p => p.Timestamps)
                         where trust.PackageDatabaseID == null
                            && trust.Replaced == false // Do not include replaced trusts
                         select trust;
            return trusts;
        }

    }
}
