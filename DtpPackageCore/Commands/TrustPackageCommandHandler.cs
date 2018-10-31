using DtpCore.Builders;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpPackageCore.Exceptions;
using DtpPackageCore.Interfaces;
using DtpStampCore.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtpPackageCore.Commands
{
    public class TrustPackageCommandHandler :
        IRequestHandler<CreateTrustPackageCommand, Package>
        //IRequestHandler<UpdateTrustPackageCommand, bool>
    {

        private IConfiguration _configuration;
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;
        private ITrustPackageService _trustPackageService;
        private ITimestampService _timestampService;
        private readonly ILogger<TrustPackageCommandHandler> logger;
        private readonly IServiceProvider _serviceProvider;

        public TrustPackageCommandHandler(IServiceProvider serviceProvider, IConfiguration configuration, ITrustDBService trustDBService, IDerivationStrategyFactory derivationStrategyFactory, ITrustPackageService trustPackageService, ITimestampService timestampService, ILogger<TrustPackageCommandHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _trustDBService = trustDBService;
            _derivationStrategyFactory = derivationStrategyFactory;
            _trustPackageService = trustPackageService;
            _timestampService = timestampService;
            this.logger = logger;
        }

        public async Task<Package> Handle(CreateTrustPackageCommand request, CancellationToken cancellationToken)
        {

            var trusts = GetTrusts();
            TrustBuilder _builder = _serviceProvider.GetRequiredService<TrustBuilder>();
            _builder.AddTrust(trusts);
            _builder.OrderTrust(); // Order trust ny ID before package ID calculation. 
            if (_builder.Package.Trusts.Count == 0)
                // No trusts found, exit
                return null;

            SignPackage(_builder);

            _builder.Package.Timestamps = _timestampService.FillIn(_builder.Package.Timestamps, _builder.Package.Id);

            _trustDBService.DBContext.Packages.Add(_builder.Package);
            await _trustDBService.DBContext.SaveChangesAsync();

            return _builder.Package;
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
            var trusts = from trust in _trustDBService.Trusts.AsNoTracking().Include(p => p.Timestamps)
                         where trust.PackageDatabaseID == 0
                            && trust.Replaced == false // Do not include replaced trusts
                         select trust;
            return trusts;
        }

    }
}
