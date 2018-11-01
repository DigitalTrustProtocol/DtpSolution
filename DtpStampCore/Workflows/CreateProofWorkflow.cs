using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Workflows;
using DtpStampCore.Extensions;
using DtpStampCore.Interfaces;
using MediatR;
using DtpCore.Repository;
using DtpCore.Model;
using DtpCore.Commands;
using DtpCore.Extensions;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Enumerations;
using System;
using DtpCore.Model.Configuration;

namespace DtpStampCore.Workflows
{
    public class CreateProofWorkflow : WorkflowContext
    {
        //private ITimestampWorkflowService _timestampWorkflowService;

        private readonly IMediator _mediator;
        private TrustDBContext _trustDBContext;
        private IConfiguration _configuration;
        private ILogger<CreateProofWorkflow> _logger;

        public BlockchainProof CurrentProof { get; private set; }
        private IMerkleTree _merkleTree;
        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IKeyValueService _keyValueService;

        public CreateProofWorkflow(IMediator mediator, TrustDBContext trustDBContext, IMerkleTree merkleTree, IBlockchainServiceFactory blockchainServiceFactory, IKeyValueService keyValueService, IConfiguration configuration, ILogger<CreateProofWorkflow> logger)
        {
            _mediator = mediator;
            _trustDBContext = trustDBContext;
            _merkleTree = merkleTree;
            _blockchainServiceFactory = blockchainServiceFactory;
            _keyValueService = keyValueService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            CurrentProof = _mediator.SendAndWait(new GetCurrentBlockchainProofCommand());

            var count = _trustDBContext.Timestamps.Where(p => p.BlockchainProofDatabaseID == CurrentProof.DatabaseID).Count();
            if (count == 0)
            {
                CombineLog(_logger, $"No proofs found");
                Wait(_configuration.TimestampInterval()); // Default 10 min
                return; // Exit workflow succesfully
            }

            // Ensure a new Proof object! 
            _mediator.SendAndWait(new AddNewBlockchainProofCommand());

            try
            {
                Merkle();
                LocalTimestamp();
            }
            catch (Exception ex)
            {
                CurrentProof.Status = ProofStatusType.Failed.ToString();
                CombineLog(_logger, $"Error in proof ID:{CurrentProof.DatabaseID} " + ex.Message);
            }
            finally
            {
                _trustDBContext.SaveChanges();
            }
            Wait(_configuration.ConfirmationWait(_configuration.Blockchain()));
        }



        public void Merkle()
        {
            var timestamps = (from p in _trustDBContext.Timestamps
                              where p.BlockchainProofDatabaseID == CurrentProof.DatabaseID
                              select p).ToList();

            foreach (var proof in timestamps)
                _merkleTree.Add(proof);

            CurrentProof.MerkleRoot = _merkleTree.Build().Hash;
            CurrentProof.Status = ProofStatusType.Waiting.ToString();

            //_trustDBService.DBContext.Timestamps.UpdateRange(timestamps); // Shoud work auto

            CombineLog(_logger, $"Proof ID:{CurrentProof.DatabaseID} Timestamp found {timestamps.Count} - Merkleroot: {CurrentProof.MerkleRoot.ConvertToHex()}");
        }


        public void LocalTimestamp()
        {
            var _fundingKeyWIF = _configuration.FundingKey(CurrentProof.Blockchain);
            var _blockchainService = _blockchainServiceFactory.GetService(CurrentProof.Blockchain);
            var fundingKey = _blockchainService.DerivationStrategy.KeyFromString(_fundingKeyWIF);

            var tempTxKey = CurrentProof.Blockchain + "_previousTx";
            var previousTx = _keyValueService.Get(tempTxKey);
            var previousTxList = (previousTx != null) ? new List<Byte[]> { previousTx } : null;

            var OutTx = _blockchainService.Send(CurrentProof.MerkleRoot, fundingKey, previousTxList);

            //OutTX needs to go to a central store for that blockchain
           _keyValueService.Set(tempTxKey, OutTx[0]);

            var merkleRootKey = _blockchainService.DerivationStrategy.GetKey(CurrentProof.MerkleRoot);
            CurrentProof.Address = _blockchainService.DerivationStrategy.GetAddress(merkleRootKey);

            CombineLog(_logger, $"Proof ID:{CurrentProof.DatabaseID} Merkle root: {CurrentProof.MerkleRoot.ConvertToHex()} has been timestamped with address: {CurrentProof.Address}");
        }

        public void RemoteTimestamp()
        {
            Failed(new MissingMethodException("Missing implementation of RemoteTimestampStep"));
        }

    }
}
