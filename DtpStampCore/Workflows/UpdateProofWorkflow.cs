using DtpCore.Workflows;
using DtpStampCore.Interfaces;
using DtpCore.Model;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using MediatR;
using DtpCore.Enumerations;
using DtpCore.Repository;
using System.ComponentModel;
using DtpCore.Notifications;
using DtpStampCore.Notifications;
using DtpStampCore.Commands;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DtpStampCore.Workflows
{
    [DisplayName("Update Proofs")]
    [Description("Update the proofs confirmation time")]
    public class UpdateProofWorkflow : WorkflowContext, ITimestampWorkflow
    {
        private readonly IMediator _mediator;
        private TrustDBContext _db;
        private IBlockchainService _blockchainService;
        private IConfiguration _configuration;
        private ILogger<UpdateProofWorkflow> _logger;


        public UpdateProofWorkflow(IMediator mediator, TrustDBContext db, IBlockchainService blockchainService, IConfiguration configuration, ILogger<UpdateProofWorkflow> logger)
        {
            _mediator = mediator;
            _db = db;
            _blockchainService = blockchainService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            var proofs = _db.Proofs
                        .Where(p => p.Status == ProofStatusType.Waiting.ToString())
                        .Select(p => p);

            foreach (var proof in proofs)
            {
                ProcessProof(proof);
            }
            _db.SaveChanges();

            Wait(_configuration.ConfirmationWait(_configuration.Blockchain()));
        }

        private void ProcessProof(BlockchainProof proof)
        {
            if(proof.Address == "Remote")
            {
                RemoteTimestamp(proof);
                return;
            }

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

            _mediator.Publish(new BlockchainProofUpdatedNotification(proof));

            var confirmationThreshold = _configuration.ConfirmationThreshold(proof.Blockchain);
            CombineLog(_logger, $"Proof ID:{proof.DatabaseID} current confirmations {proof.Confirmations} of {confirmationThreshold}");

            if (proof.Confirmations >= confirmationThreshold)
            {
                proof.Status = ProofStatusType.Done.ToString();
                _mediator.Publish(new BlockchainProofDoneNotification(proof));
            }
        }

        private void RemoteTimestamp(BlockchainProof proof)
        {
            var uri = _configuration.RemoteServer();
            uri = uri.Append($"/api/timestamp/{proof.MerkleRoot.ConvertToBase64()}");

            using (var client = new WebClient())
            {
                var json = client.DownloadString(uri);
                if(!string.IsNullOrEmpty(json))
                {
                    var timestamp = JsonConvert.DeserializeObject<Timestamp>(json);
                    if(timestamp.Proof != null)
                    {
                        if(timestamp.Proof.Confirmations > 0)
                        {
                            proof.Receipt = ByteExtensions.Combine(timestamp.Path, timestamp.Proof.Receipt);
                            proof.Address = timestamp.Proof.Address;
                            proof.Confirmations = timestamp.Proof.Confirmations;
                            proof.Blockchain = timestamp.Proof.Blockchain;
                        }
                    }
                }

            }
        }
    }
}
