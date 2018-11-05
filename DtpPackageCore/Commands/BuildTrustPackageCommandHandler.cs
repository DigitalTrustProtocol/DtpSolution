using DtpCore.Builders;
using DtpCore.Commands;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
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
        IRequestHandler<BuildTrustPackageCommand, Package>
    {
        private IMediator _mediator;

        private IConfiguration _configuration;
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private ITrustPackageService _trustPackageService;
        private readonly ILogger<BuildTrustPackageCommandHandler> logger;
        private readonly IServiceProvider _serviceProvider;

        public BuildTrustPackageCommandHandler(IMediator mediator, IServiceProvider serviceProvider, IConfiguration configuration, ITrustDBService trustDBService, IDerivationStrategyFactory derivationStrategyFactory, ITrustPackageService trustPackageService, ILogger<BuildTrustPackageCommandHandler> logger)
        {
            _mediator = mediator;

            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _trustDBService = trustDBService;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            this.logger = logger;
        }

        public Task<Package> Handle(BuildTrustPackageCommand request, CancellationToken cancellationToken)
        {

            var trusts = GetTrusts();
            TrustBuilder _builder = new TrustBuilder(_serviceProvider); // _serviceProvider.GetRequiredService<TrustBuilder>();
            _builder.AddTrust(trusts);
            _builder.OrderTrust(); // Order trust ny ID before package ID calculation. 
            if (_builder.Package.Trusts.Count == 0)
                // No trusts found, exit
                return Task.FromCanceled<Package>(cancellationToken);

            SignPackage(_builder);

            var timestamp = _mediator.SendAndWait(new CreateTimestampCommand { Source = _builder.Package.Id });
            _builder.Package.Timestamps = _builder.Package.Timestamps ?? new List<Timestamp>();
            _builder.Package.Timestamps.Add(timestamp);

            _trustDBService.DBContext.Packages.Add(_builder.Package);
            _trustDBService.DBContext.SaveChanges();

            _mediator.Publish(new TrustPackageBuildNotification(_builder.Package));

            return Task.FromResult(_builder.Package);
        }

        //public async Task<bool> Handle(UpdateTrustPackageCommand request, CancellationToken cancellationToken)
        //{
        //    //if (!FileRepository.Exist(request.Name))
        //    //    throw new TrustPackageMissingException($"Trust package {request.Name} missing, cannot update");

        //    //var content = JsonConvert.SerializeObject(request.TrustPackage, Formatting.Indented);

        //    //await FileRepository.WriteFileAsync(request.Name, content);

        //    return true;
        //}


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
            var trusts = from trust in _trustDBService.Trusts.Include(p => p.Timestamps)
                         where trust.PackageDatabaseID == null
                            && trust.Replaced == false // Do not include replaced trusts
                         select trust;
            return trusts;
        }

    }
}
