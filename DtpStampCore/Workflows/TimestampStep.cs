using System.Linq;
using TrustchainCore.Workflows;
using TrustchainCore.Interfaces;
using TruststampCore.Interfaces;
using Microsoft.Extensions.Logging;
using TrustchainCore.Extensions;
using TruststampCore.Enumerations;
using Microsoft.Extensions.Configuration;
using TruststampCore.Extensions;
using TrustchainCore.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TruststampCore.Workflows
{
    public class TimestampStep : WorkflowStep, ITimestampStep
    {
        private IBlockchainServiceFactory _blockchainServiceFactory;
        private ITrustDBService _trustDBService;
        private IMerkleTree _merkleTree;
        private IConfiguration _configuration;
        private ILogger<TimestampStep> _logger;
        private IBlockchainService blockchainService;
        private IKeyValueService _keyValueService;

        //[JsonIgnore]
        //public IList<byte[]> OutTx { get; set; }

        [JsonIgnore]
        public BlockchainProof TimestampProof
        {
            get
            {
                return ((ITimestampWorkflow)Context).Proof;
            }
        }

        [JsonIgnore]
        public string FundingKeyWIF
        {
            get
            {
                return _configuration.FundingKey(TimestampProof.Blockchain);
            }
        }

        public TimestampStep(IBlockchainServiceFactory blockchainServiceFactory, ITrustDBService trustDBService,
            IKeyValueService keyValueService,
            IMerkleTree merkleTree, IConfiguration configuration, ILogger<TimestampStep> logger)
        {
            _blockchainServiceFactory = blockchainServiceFactory;
            _trustDBService = trustDBService;
            _keyValueService = keyValueService;
            _merkleTree = merkleTree;
            _configuration = configuration;
            _logger = logger;
            blockchainService = _blockchainServiceFactory.GetService(TimestampProof.Blockchain);
        }

        public override void Execute()
        {
            // Ensure MerkleRoot
            if (TimestampProof.MerkleRoot == null || TimestampProof.MerkleRoot.Length == 0)
            {
                BuildMerkleTree();
            }

            // Check if the address already has been activated
            var blockchainTimestamp = blockchainService.GetTimestamp(TimestampProof.MerkleRoot);

            TimestampProof.Confirmations = blockchainTimestamp.Confirmations;
            if (TimestampProof.Confirmations >= 0)
            {
                ProcessConfirmation();
                return;
            }

            if (TimestampProof.Confirmations == -1) // No timestamp on merkleRoot
            {
                TimestampMerkeRoot(TimestampProof, blockchainService);
                return;
            }


        }



        public void BuildMerkleTree()
        { 
            var timestamps = (from p in _trustDBService.Timestamps
                          where p.WorkflowID == Context.ID
                          select p).ToList();

            if(timestamps.Count == 0)
            {
                return; // Exit workflow succesfully
            }

            foreach (var timestamp in timestamps)
            {
                _merkleTree.Add(timestamp);
            }

            TimestampProof.MerkleRoot = _merkleTree.Build().Hash;
            CombineLog(_logger, $"Proof found {timestamps.Count} - Merkleroot: {TimestampProof.MerkleRoot.ConvertToHex()}");
        }

        public void ProcessConfirmation()
        {
            var confirmationThreshold = _configuration.ConfirmationThreshold(TimestampProof.Blockchain);
            if (TimestampProof.Confirmations < confirmationThreshold)
            {
                CombineLog(_logger, $"Current confirmations {TimestampProof.Confirmations} of {confirmationThreshold}");
                Context.Wait(_configuration.ConfirmationWait(TimestampProof.Blockchain)); // Run again, but wait
            }
            else
            {
                TimestampProof.Status = TimestampProofStatusType.Done.ToString();
                Context.AddStep<ISuccessStep>(); // Workflow done!
            }
        }

        public void TimestampMerkeRoot(BlockchainProof timestampProof, IBlockchainService blockchainService)
        {
            if (String.IsNullOrWhiteSpace(FundingKeyWIF))
            {
                CombineLog(_logger, $"No server key provided, using remote timestamping");
                RemoteTimestamp();
                return;
            }

            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);
            if (blockchainService.VerifyFunds(fundingKey, null) == 0)
            {
                CombineLog(_logger, $"Available funds detected on funding key, using local timestamping");
                // There are funds on the key
                LocalTimestamp();
                return;
            }
            else
            {
                // There are no funds, use remote timestamping.
                CombineLog(_logger, $"There are no funds, using remote timestamping");
                RemoteTimestamp();
                return;
            }
        }

        public void LocalTimestamp()
        {
            var fundingKey = blockchainService.DerivationStrategy.KeyFromString(FundingKeyWIF);

            var merkleRootKey = blockchainService.DerivationStrategy.GetKey(TimestampProof.MerkleRoot);
            TimestampProof.Address = blockchainService.DerivationStrategy.GetAddress(merkleRootKey);

            var tempTxKey = TimestampProof.Blockchain + "_previousTx";
            var previousTx = _keyValueService.Get(tempTxKey);
            var previousTxList = (previousTx != null) ? new List<Byte[]> { previousTx } : null;

            var OutTx = blockchainService.Send(TimestampProof.MerkleRoot, fundingKey, previousTxList);

            // OutTX needs to go to a central store for that blockchain
            _keyValueService.Set(tempTxKey, OutTx[0]);

            var merkleAddressString = blockchainService.DerivationStrategy.StringifyAddress(merkleRootKey);
            CombineLog(_logger, $"Merkle root: {TimestampProof.MerkleRoot.ConvertToHex()} has been timestamped with address: {merkleAddressString}");

            Context.Wait(_configuration.ConfirmationWait(TimestampProof.Blockchain)); // Run again, but wait
        }


        public void RemoteTimestamp()
        {
            TimestampProof.Remote = new BlockchainProof();
            // Missing logic
            Context.Wait(_configuration.ConfirmationWait(TimestampProof.Blockchain)); // Run again, but wait
        }

    }
}
