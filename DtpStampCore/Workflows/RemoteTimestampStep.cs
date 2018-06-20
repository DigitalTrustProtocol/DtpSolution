using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrustchainCore.Workflows;
using TruststampCore.Enumerations;
using TruststampCore.Extensions;
using TruststampCore.Interfaces;

namespace TruststampCore.Workflows
{
    public class RemoteTimestampStep : WorkflowStep, IRemoteTimestampStep
    {

        public int RetryAttempts { get; set; }

        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IConfiguration _configuration;
        private ILogger<RemoteTimestampStep> _logger;
        private IBlockchainService _blockchainService;

        public RemoteTimestampStep(IBlockchainServiceFactory blockchainServiceFactory, IConfiguration configuration, ILogger<RemoteTimestampStep> logger)
        {
            _blockchainServiceFactory = blockchainServiceFactory;
            _configuration = configuration;
            _logger = logger;
            _blockchainService = _blockchainServiceFactory.GetService(_configuration.Blockchain());

        }
        public override void Execute()
        {
            var timestampProof = ((ITimestampWorkflow)Context).Proof;
            timestampProof.Status = TimestampProofStatusType.Done.ToString();
            timestampProof.Remote = new TrustchainCore.Model.BlockchainProof();

        }
    }
}
