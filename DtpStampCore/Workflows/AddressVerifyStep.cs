using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using TrustchainCore.Extensions;
using TrustchainCore.Interfaces;
using TrustchainCore.Model;
using TrustchainCore.Workflows;
using TruststampCore.Enumerations;
using TruststampCore.Extensions;
using TruststampCore.Interfaces;

namespace TruststampCore.Workflows
{
    public class AddressVerifyStep : WorkflowStep, IAddressVerifyStep
    {
        public int RetryAttempts { get; set; }

        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IConfiguration _configuration;
        private ILogger<AddressVerifyStep> _logger;

        public AddressVerifyStep(IBlockchainServiceFactory blockchainServiceFactory, IConfiguration configuration, ILogger<AddressVerifyStep> logger)
        {
            _blockchainServiceFactory = blockchainServiceFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            try
            {
                RetryAttempts++;

                var proof = ((ITimestampWorkflow)Context).Proof;


                var blockchainService = _blockchainServiceFactory.GetService(proof.Blockchain);
                var blockchainTimestamp = blockchainService.GetTimestamp(proof.MerkleRoot);
                proof.Confirmations = blockchainTimestamp.Confirmations;
                
                if (proof.Confirmations >= 0)
                {
                    var confirmationThreshold = _configuration.ConfirmationThreshold(proof.Blockchain);
                    if (proof.Confirmations < confirmationThreshold)
                    {
                        CombineLog(_logger, $"Current confirmations {proof.Confirmations} of {confirmationThreshold}");
                    }
                    else
                    {
                        Context.RunStep<ISuccessStep>(); // Workflow done!
                        return;
                    }
                }

                if(RetryAttempts > 60 * 24 * 7) // a week.
                {
                    throw new ApplicationException($"To many RetryAttempts: {RetryAttempts}, cancelling workflow.");
                }

                Context.Wait(_configuration.ConfirmationWait(proof.Blockchain));
            }
            catch (Exception ex)
            {
                if (RetryAttempts >= 60)
                    throw;
                
                _logger.LogError(Context.ID, ex, "Execute failed");
                Context.Log($"Step: {this.GetType().Name} has failed with an error: {ex.Message}");
                Context.Wait(_configuration.StepRetryAttemptWait());
            }
        }

        //private void TimestampMerkeRoot(BlockchainProof timestampProof, IBlockchainService blockchainService)
        //{
        //    var fundingKeyWIF = _configuration.FundingKey(timestampProof.Blockchain);
        //    if (String.IsNullOrWhiteSpace(fundingKeyWIF))
        //    {
        //        CombineLog(_logger, $"No server key provided, using remote timestamping");
        //        Context.RunStep<IRemoteTimestampStep>();
        //        return;
        //    }

        //    var fundingKey = blockchainService.CryptoStrategy.KeyFromString(fundingKeyWIF);
        //    if (blockchainService.VerifyFunds(fundingKey, null) == 0)
        //    {
        //        CombineLog(_logger, $"Available funds detected on funding key, using local timestamping");
        //        // There are funds on the key
        //        Context.RunStep<ILocalTimestampStep>();
        //        return;
        //    }
        //    else
        //    {
        //        // There are no funds, use remote timestamping.
        //        CombineLog(_logger, $"There are no funds, using remote timestamping");
        //        Context.RunStep<IRemoteTimestampStep>();
        //        return;
        //    }
        //}
    }
}
