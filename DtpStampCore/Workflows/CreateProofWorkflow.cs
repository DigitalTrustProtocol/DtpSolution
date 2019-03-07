using Microsoft.Extensions.Configuration;
using DtpCore.Workflows;
using DtpStampCore.Extensions;
using DtpStampCore.Interfaces;
using MediatR;
using DtpCore.Repository;
using DtpCore.Model;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Enumerations;
using System;
using System.ComponentModel;
using DtpStampCore.Commands;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DtpCore.Model.Database;

namespace DtpStampCore.Workflows
{
    [DisplayName("Create Proof for timestamps")]
    [Description("Calculated the Timestamps sources into a merke tree and put the root on the blockchain")]
    public class CreateProofWorkflow : WorkflowContext
    {
        //private ITimestampWorkflowService _timestampWorkflowService;

        private readonly IMediator _mediator;
        private TrustDBContext _db;
        private IConfiguration _configuration;
        private ILogger<CreateProofWorkflow> _logger;

        //public BlockchainProof CurrentProof { get; private set; }
        private IMerkleTree _merkleTree;
        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IKeyValueService _keyValueService;

        public CreateProofWorkflow(IMediator mediator, TrustDBContext db, IMerkleTree merkleTree, IBlockchainServiceFactory blockchainServiceFactory, IKeyValueService keyValueService, IConfiguration configuration, ILogger<CreateProofWorkflow> logger)
        {
            _mediator = mediator;
            _db = db;
            _merkleTree = merkleTree;
            _blockchainServiceFactory = blockchainServiceFactory;
            _keyValueService = keyValueService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            // Getting the current aggregator for timestamps
            var proof = new BlockchainProof();
            // TODO: Support more than one blockchain type!!
            proof.Timestamps = _db.Timestamps.Where(p => p.ProofDatabaseID == null).ToList();

            if (proof.Timestamps.Count == 0)
            {
                CombineLog(_logger, $"No timestamps found");
                Wait(_configuration.TimestampInterval()); // Default 10 min
                return; // Exit workflow succesfully
            }

            //CurrentProof = _mediator.SendAndWait(new CurrentBlockchainProofQuery());
            // Ensure a new Proof object! 
            try
            {
                Merkle(proof);

                // If funding key is available then use, local timestamping.
                LocalTimestamp(proof);

                proof.Status = ProofStatusType.Waiting.ToString();

                _db.Proofs.Add(proof);
                // Otherwise use the remote timestamp from trust.dance.
            }
            catch (Exception ex)
            {
                proof.Status = ProofStatusType.Failed.ToString();
                CombineLog(_logger, $"Error in proof ID:{proof.DatabaseID} " + ex.Message+":"+ex.StackTrace);
            }
            finally
            {
                var result = _db.SaveChangesAsync().GetAwaiter().GetResult();
                _logger.LogTrace($"CreateProofWorkflow save with result code: {result}");
                
            }
            Wait(_configuration.ConfirmationWait(_configuration.Blockchain()));
        }



        public void Merkle(BlockchainProof proof)
        {
            foreach (var item in proof.Timestamps)
                _merkleTree.Add(item);


            proof.MerkleRoot = _merkleTree.Build().Hash;

            CombineLog(_logger, $"Proof ID:{proof.DatabaseID} Timestamp found {proof.Timestamps.Count} - Merkleroot: {proof.MerkleRoot.ConvertToHex()}");
        }


        public void LocalTimestamp(BlockchainProof proof)
        {
            var _fundingKeyWIF = _configuration.FundingKey(proof.Blockchain);
            var _blockchainService = _blockchainServiceFactory.GetService(proof.Blockchain);
            var fundingKey = _blockchainService.DerivationStrategy.KeyFromString(_fundingKeyWIF);

            var tempTxKey = proof.Blockchain + "_previousTx";
            var previousTx = _keyValueService.Get(tempTxKey);
            var previousTxList = (previousTx != null) ? new List<Byte[]> { previousTx } : null;

            var OutTx = _blockchainService.Send(proof.MerkleRoot, fundingKey, previousTxList);

            //OutTX needs to go to a central store for that blockchain
           _keyValueService.Set(tempTxKey, OutTx[0]);

            var merkleRootKey = _blockchainService.DerivationStrategy.GetKey(proof.MerkleRoot);
            proof.Address = _blockchainService.DerivationStrategy.GetAddress(merkleRootKey);

            CombineLog(_logger, $"Proof ID:{proof.DatabaseID} Merkle root: {proof.MerkleRoot.ConvertToHex()} has been timestamped with address: {proof.Address}");
        }

        public void RemoteTimestamp()
        {
            Failed(new MissingMethodException("Missing implementation of RemoteTimestampStep"));
        }

    }
}
