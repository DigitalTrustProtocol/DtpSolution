using DtpGraphCore.Interfaces;
using DtpCore.Services;
using DtpCore.Workflows;
using System;
using System.Linq;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DtpCore.Repository;
using DtpCore.Interfaces;
using DtpCore.Builders;
using DtpStampCore.Extensions;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model.Configuration;
using System.Text;

namespace DtpGraphCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    public class TrustPackageWorkflow : WorkflowContext, ITrustPackageWorkflow
    {

        public int LastTrustDatabaseID { get; set; }

        private ILogger<TrustPackageWorkflow> _logger;
        private IConfiguration _configuration;
        private PublicFileRepository _publicFileRepository;
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;


        public TrustPackageWorkflow()
        {
        }

        public override void Execute()
        {
            _logger = WorkflowService.ServiceProvider.GetRequiredService<ILogger<TrustPackageWorkflow>>();
            _configuration = WorkflowService.ServiceProvider.GetRequiredService<IConfiguration>();
            _publicFileRepository = WorkflowService.ServiceProvider.GetRequiredService<PublicFileRepository>();
            _trustDBService = WorkflowService.ServiceProvider.GetRequiredService<ITrustDBService>();
            _derivationStrategyFactory = WorkflowService.ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            
            // Set now
            var now = DateTime.Now.ToUniversalTime();

            // Create a name and check it
            var name = CreatePackageName(now);
            if (_publicFileRepository.Exist(name))
            {
                CombineLog(_logger, $"Package file {name} already exist. ");
                return;

            }

            var serverSection = _configuration.GetModel<ServerSection>();

            // Get all trusts from LastTrustDatabaseID to now
            var trusts = from trust in _trustDBService.Trusts.AsNoTracking().Include(p => p.Timestamps)
                         where trust.DatabaseID > LastTrustDatabaseID
                            && trust.Replaced == false // Do not include replaced trusts
                         select trust;

            var builder = new TrustBuilder(WorkflowService.ServiceProvider);
            var nextID = LastTrustDatabaseID;

            foreach (var trust in trusts)
            {
                builder.AddTrust(trust);

                if (trust.DatabaseID > nextID)
                    nextID = trust.DatabaseID;
            }

            if (builder.Package.Trusts.Count == 0)
                // No trusts found, exit
                return;

            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);

            var key = scriptService.GetKey(Encoding.UTF8.GetBytes(serverSection.GetSecureKeyword()));
            builder.Package.Server.Address = scriptService.GetAddress(key);
            builder.Package.ServerSign = (byte[] packageId) => scriptService.SignMessage(key, packageId);

            builder.Build();
            builder.SignServer();

            var packageContent = builder.ToString();
            _publicFileRepository.WriteFile(name, packageContent);

            CombineLog(_logger, $"Package file {name} created with {builder.Package.Trusts.Count} trusts.");

            LastTrustDatabaseID = nextID;

            Wait(_configuration.TrustPackageWorkflowInterval()); // Never end the workflow
        }

        private string CreatePackageName(DateTime now)
        {
            return $"Package_trustdance_{now.ToString("yyyyMMdd_hhmmss")}.json";
        }
    }

}
