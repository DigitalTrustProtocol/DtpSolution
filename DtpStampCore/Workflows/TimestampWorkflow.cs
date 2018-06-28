using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DtpCore.Workflows;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpCore.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.Logging;
using DtpStampCore.Enumerations;
using System.Collections.Generic;
using DtpStampCore.Model;

namespace DtpStampCore.Workflows
{
    public class TimestampWorkflow : WorkflowContext, ITimestampWorkflow
    {
        public enum TimestampStates { Synchronization, Merkle, Timestamp, LocalTimestamp, RemoteTimestamp, AddressVerify }
        
        [JsonProperty(PropertyName = "proof", NullValueHandling = NullValueHandling.Ignore)]
        public BlockchainProof Proof { get; set; }

        public int RetryAttempts { get; set; }

        public IList<byte[]> OutTx { get; set; }

        public TimestampStates CurrentState { get; set; }
        
        [JsonIgnore]
        public Action MethodCallback { get; set; }
        [JsonIgnore]
        public bool StopExecution { get; set; }

        private ITimestampSynchronizationService _timestampSynchronizationService;
        private IConfiguration _configuration;

        private ITrustDBService _trustDBService;
        private IMerkleTree _merkleTree;

        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IKeyValueService _keyValueService;


        private IBlockchainService _blockchainService;
        private string _fundingKeyWIF;
        private AddressTimestamp _blockchainTimestamp;

        private ILogger<TimestampWorkflow> _logger;


        public TimestampWorkflow()
        {
            CurrentState = TimestampStates.Synchronization;
        }

        public void SetCurrentState(TimestampStates state)
        {
            CurrentState = state;
        }

        public override void Execute()
        {
            Init();

            var time = DateTime.Now.ToUnixTime();

            while(Container.Active && Container.NextExecution < time && !StopExecution)
            {
                CallMethod(Enum.GetName(typeof(TimestampStates), CurrentState));

                if(MethodCallback != null)
                {
                    MethodCallback.Invoke();
                }
            }
        }

        private void Init()
        {
            // Dependicies
            _configuration = WorkflowService.ServiceProvider.GetRequiredService<IConfiguration>();
            // Init
            if (Proof == null)
            {
                Proof = new BlockchainProof
                {
                    Blockchain = _configuration.Blockchain()
                };
            }

            _timestampSynchronizationService = WorkflowService.ServiceProvider.GetRequiredService<ITimestampSynchronizationService>();
            _trustDBService = WorkflowService.ServiceProvider.GetRequiredService<ITrustDBService>();
            _merkleTree = WorkflowService.ServiceProvider.GetRequiredService<IMerkleTree>();
            _blockchainServiceFactory = WorkflowService.ServiceProvider.GetRequiredService<IBlockchainServiceFactory>();
            _keyValueService = WorkflowService.ServiceProvider.GetRequiredService<IKeyValueService>();
            _logger = WorkflowService.ServiceProvider.GetRequiredService<ILogger<TimestampWorkflow>>();
            _blockchainService = _blockchainServiceFactory.GetService(Proof.Blockchain);
            _fundingKeyWIF = _configuration.FundingKey(Proof.Blockchain);
        }

        public void Synchronization()
        {
            if (Container.DatabaseID == _timestampSynchronizationService.CurrentWorkflowID)
            {
                Wait(_configuration.TimestampInterval());
            }
            else
            {
                CurrentState = TimestampStates.Merkle;
            }
        }


        public void Merkle()
        {
            var timestamps = (from p in _trustDBService.Timestamps
                            where p.WorkflowID == Container.DatabaseID
                            select p).ToList();

            if (timestamps.Count == 0)
            {
                CombineLog(_logger, $"No proofs found");
                Success();
                return; // Exit workflow succesfully
            }

            foreach (var proof in timestamps)
                _merkleTree.Add(proof);

            Proof.MerkleRoot = _merkleTree.Build().Hash;
            Proof.Status = TimestampProofStatusType.Waiting.ToString();

            _trustDBService.DBContext.Timestamps.UpdateRange(timestamps);
            _trustDBService.DBContext.SaveChanges();

            CombineLog(_logger, $"Timestamp found {timestamps.Count} - Merkleroot: {Proof.MerkleRoot.ConvertToHex()}");

            SetCurrentState(TimestampStates.Timestamp);
        }

        public void Timestamp()
        {
            UpdateProofTimestamp();

            if (Proof.Confirmations >= 0)
            {
                SetCurrentState(TimestampStates.AddressVerify);
                return;
            }

            if (String.IsNullOrWhiteSpace(_fundingKeyWIF))
            {
                _logger.DateInformation(Container.DatabaseID, $"No server key provided, using remote timestamping");
                SetCurrentState(TimestampStates.RemoteTimestamp);
            }
            else
            {
                SetCurrentState(TimestampStates.LocalTimestamp);
            }

        }

        private void UpdateProofTimestamp()
        {
            if (_blockchainTimestamp == null)
            {
                _blockchainTimestamp = _blockchainService.GetTimestamp(Proof.MerkleRoot);
                Proof.Confirmations = _blockchainTimestamp.Confirmations;
                Proof.BlockTime = _blockchainTimestamp.Time;
            }
        }

        public void LocalTimestamp()
        {
            var fundingKey = _blockchainService.DerivationStrategy.KeyFromString(_fundingKeyWIF);

            var tempTxKey = Proof.Blockchain + "_previousTx";
            var previousTx = _keyValueService.Get(tempTxKey);
            var previousTxList = (previousTx != null) ? new List<Byte[]> { previousTx } : null;

            OutTx = _blockchainService.Send(Proof.MerkleRoot, fundingKey, previousTxList);

            // OutTX needs to go to a central store for that blockchain
            _keyValueService.Set(tempTxKey, OutTx[0]);

            var merkleRootKey = _blockchainService.DerivationStrategy.GetKey(Proof.MerkleRoot);
            Proof.Address = _blockchainService.DerivationStrategy.GetAddress(merkleRootKey);

            CombineLog(_logger, $"Merkle root: {Proof.MerkleRoot.ConvertToHex()} has been timestamped with address: {Proof.Address}");

            SetCurrentState(TimestampStates.AddressVerify);
            Wait(_configuration.ConfirmationWait(Proof.Blockchain));
        }

        public void RemoteTimestamp()
        {
            Failed(new MissingMethodException("Missing implementation of RemoteTimestampStep"));
        }

        public void AddressVerify()
        {
            try
            {
                RetryAttempts++;

                UpdateProofTimestamp(); 

                if (Proof.Confirmations >= 0)
                {
                    var confirmationThreshold = _configuration.ConfirmationThreshold(Proof.Blockchain);
                    if (Proof.Confirmations < confirmationThreshold)
                    {
                        CombineLog(_logger, $"Current confirmations {Proof.Confirmations} of {confirmationThreshold}");
                    }
                    else
                    {
                        Success(); // Workflow done!
                        return;
                    }
                }

                if (RetryAttempts > 60 * 24 * 7) // a week.
                {
                    throw new ApplicationException($"To many RetryAttempts: {RetryAttempts}, cancelling workflow.");
                }

                Wait(_configuration.ConfirmationWait(Proof.Blockchain));
            }
            catch (Exception ex)
            {
                if (RetryAttempts >= 60)
                    throw;

                _logger.LogError(Container.DatabaseID, ex, "Execute failed");
                Log($"Step: {this.GetType().Name} has failed with an error: {ex.Message}");
                Wait(_configuration.StepRetryAttemptWait());
            }
        }
    }
}
