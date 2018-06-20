using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TrustchainCore.Extensions;
using TrustchainCore.Interfaces;
using TrustchainCore.Workflows;
using TruststampCore.Enumerations;
using TruststampCore.Extensions;
using TruststampCore.Interfaces;

namespace TruststampCore.Workflows
{
    public class LocalTimestampStep : WorkflowStep, ILocalTimestampStep
    {
        public int RetryAttempts { get; set; }
        public IList<byte[]> OutTx { get; set; }

        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IKeyValueService _keyValueService;
        private IConfiguration _configuration;
        private ILogger<LocalTimestampStep> _logger;

        public LocalTimestampStep(IBlockchainServiceFactory blockchainServiceFactory, IKeyValueService keyValueService, IConfiguration configuration, ILogger<LocalTimestampStep> logger)
        {
            _blockchainServiceFactory = blockchainServiceFactory;
            _keyValueService = keyValueService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            var proof = ((ITimestampWorkflow)Context).Proof;
            //proof.Confirmations = 0;

            var fundingKeyWIF = _configuration.FundingKey(proof.Blockchain);
            if (String.IsNullOrWhiteSpace(fundingKeyWIF))
            {
                _logger.DateInformation(Context.ID, $"No server key provided, using remote timestamping");
                Context.RunStep<IRemoteTimestampStep>(); // Now run remote step
                return;
            }

            var blockchainService = _blockchainServiceFactory.GetService(proof.Blockchain);
            var blockchainTimestamp = blockchainService.GetTimestamp(proof.MerkleRoot);
            proof.Confirmations = blockchainTimestamp.Confirmations;

            if (proof.Confirmations > -1) // Already timestamp on merkleRoot
            {
                Context.RunStep<IAddressVerifyStep>(); // Now run verify step
                return;
            }

            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(fundingKeyWIF);

            var tempTxKey = proof.Blockchain + "_previousTx";
            var previousTx = _keyValueService.Get(tempTxKey);
            var previousTxList = (previousTx != null) ? new List<Byte[]> { previousTx } : null;

            OutTx = blockchainService.Send(proof.MerkleRoot, fundingKey, previousTxList);

            // OutTX needs to go to a central store for that blockchain
            _keyValueService.Set(tempTxKey, OutTx[0]);

            var merkleRootKey = blockchainService.DerivationStrategy.GetKey(proof.MerkleRoot);
            proof.Address = blockchainService.DerivationStrategy.GetAddress(merkleRootKey);

            var merkleAddressString = blockchainService.DerivationStrategy.StringifyAddress(merkleRootKey);
            CombineLog(_logger, $"Merkle root: {proof.MerkleRoot.ConvertToHex()} has been timestamped with address: {merkleAddressString}");

            Context.RunStep<IAddressVerifyStep>(_configuration.ConfirmationWait(proof.Blockchain));
        }
    }
}
