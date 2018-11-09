﻿using DtpCore.Workflows;
using DtpStampCore.Interfaces;
using DtpCore.Model;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using MediatR;
using DtpCore.Commands;
using DtpCore.Enumerations;
using DtpCore.Repository;
using System.ComponentModel;
using DtpCore.Notifications;

namespace DtpStampCore.Workflows
{
    [DisplayName("Update Proofs")]
    [Description("Update the proofs confirmation time")]
    public class UpdateProofWorkflow : WorkflowContext, ITimestampWorkflow
    {
        private readonly IMediator _mediator;
        private TrustDBContext _trustDBContext;
        private IBlockchainService _blockchainService;
        private IConfiguration _configuration;
        private ILogger<UpdateProofWorkflow> _logger;


        public UpdateProofWorkflow(IMediator mediator, TrustDBContext trustDBContext, IBlockchainService blockchainService, IConfiguration configuration, ILogger<UpdateProofWorkflow> logger)
        {
            _mediator = mediator;
            _trustDBContext = trustDBContext;
            _blockchainService = blockchainService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            // Get all waiting and process them.
            var proofs = _mediator.SendAndWait(new WaitingBlockchainProofQuery());

            foreach (var proof in proofs)
            {
                ProcessProof(proof);
            }
            _trustDBContext.SaveChanges();

            Wait(_configuration.ConfirmationWait(_configuration.Blockchain()));
        }

        private void ProcessProof(BlockchainProof proof)
        {
            var addressTimestamp = _blockchainService.GetTimestamp(proof.MerkleRoot);
            if(addressTimestamp == null)
            {
                proof.Status = ProofStatusType.Failed.ToString();
                CombineLog(_logger, $"Proof ID:{proof.DatabaseID} failed with no blockchain transaction found.");
                return;
            }

            proof.RetryAttempts++;
            if (proof.RetryAttempts >= 60)
            {
                proof.Status = ProofStatusType.Failed.ToString();
                CombineLog(_logger, $"Proof ID:{proof.DatabaseID} failed with to many attempts to get a confirmation.");
                return;
            }


            proof.Confirmations = addressTimestamp.Confirmations;
            proof.BlockTime = addressTimestamp.Time;

            var confirmationThreshold = _configuration.ConfirmationThreshold(proof.Blockchain);
            if (proof.Confirmations >= confirmationThreshold)
            {
                proof.Status = ProofStatusType.Done.ToString();
            }
            CombineLog(_logger, $"Proof ID:{proof.DatabaseID} current confirmations {proof.Confirmations} of {confirmationThreshold}");

            _mediator.Publish(new BlockchainProofUpdatedNotification(proof));
        }


        //public override void Execute()
        //{
        //    Init();

        //    var time = DateTime.Now.ToUnixTime();

        //    while(Container.Active && Container.NextExecution < time && !StopExecution)
        //    {
        //        CallMethod(Enum.GetName(typeof(TimestampStates), CurrentState));

        //        if(MethodCallback != null)
        //        {
        //            MethodCallback.Invoke();
        //        }
        //    }
        //}
    
    }
}