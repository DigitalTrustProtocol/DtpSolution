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
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace DtpGraphCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    public class TrustPackageWorkflow : WorkflowContext
    {

        public int LastTrustDatabaseID { get; set; }

        private ILogger<TrustPackageWorkflow> _logger;
        private IConfiguration _configuration;
        private ITrustDBService _trustDBService;
        private IDerivationStrategyFactory _derivationStrategyFactory;

        [JsonIgnore]
        [NotMapped]
        public TrustBuilder Builder { get; set; }

        [JsonIgnore]
        [NotMapped]
        public IPublicFileRepository FileRepository { get; set; } 

        public TrustPackageWorkflow()
        {
        }

        public override void Execute()
        {
            _logger = WorkflowService.ServiceProvider.GetRequiredService<ILogger<TrustPackageWorkflow>>();
            _configuration = WorkflowService.ServiceProvider.GetRequiredService<IConfiguration>();
            FileRepository = WorkflowService.ServiceProvider.GetRequiredService<IPublicFileRepository>();
            _trustDBService = WorkflowService.ServiceProvider.GetRequiredService<ITrustDBService>();
            _derivationStrategyFactory = WorkflowService.ServiceProvider.GetRequiredService<IDerivationStrategyFactory>();
            
            // Set now
            var now = DateTime.Now.ToUniversalTime();

            // Create a name and check it
            var name = CreatePackageName(now);
            if (FileRepository.Exist(name))
            {
                CombineLog(_logger, $"Package file {name} already exist. ");
                return;

            }

            var serverSection = _configuration.GetModel(new ServerSection());

            // Get all trusts from LastTrustDatabaseID to now
            var trusts = from trust in _trustDBService.Trusts.AsNoTracking().Include(p => p.Timestamps)
                         where trust.DatabaseID > LastTrustDatabaseID
                            && trust.Replaced == false // Do not include replaced trusts
                         orderby trust.Created
                         select trust;

            Builder = new TrustBuilder(WorkflowService.ServiceProvider);
            var nextID = LastTrustDatabaseID;

            foreach (var trust in trusts)
            {
                Builder.AddTrust(trust);

                if (trust.DatabaseID > nextID)
                    nextID = trust.DatabaseID;
            }

            if (Builder.Package.Trusts.Count == 0)
                // No trusts found, exit
                return;

            var scriptService = _derivationStrategyFactory.GetService(serverSection.Type);

            var key = scriptService.GetKey(Encoding.UTF8.GetBytes(serverSection.GetSecureKeyword()));
            Builder.SetServer(scriptService.GetAddress(key), null, (byte[] packageId) => scriptService.SignMessage(key, packageId));

            Builder.Build();
            Builder.SignServer();

            var packageContent = Builder.ToString();
            FileRepository.WriteFile(name, packageContent);

            CombineLog(_logger, $"Package file {name} created with {Builder.Package.Trusts.Count} trusts.");

            LastTrustDatabaseID = nextID;

            Wait(_configuration.TrustPackageWorkflowInterval()); // Never end the workflow
        }

        private string CreatePackageName(DateTime now)
        {
            return $"Package_trustdance_{now.ToString("yyyyMMdd_hhmmss")}.json";
        }
    }

}
