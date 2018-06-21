using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using System.Collections;
using DtpCore.Interfaces;
using DtpStampCore.Interfaces;
using DtpCore.Services;
using DtpCore.Extensions;
using DtpStampCore.Workflows;

namespace DtpServer.Pages.Timestamps
{
    public class DetailsModel : PageModel
    {
        private readonly TrustDBContext _context;
        private readonly IMerkleTree _merkleTree;
        private readonly IBlockchainServiceFactory _blockchainServiceFactory;
        private readonly IWorkflowService _workflowService;


        public DetailsModel(TrustDBContext context, IMerkleTree merkleTree, IBlockchainServiceFactory blockchainServiceFactory, IWorkflowService workflowService)
        {
            _context = context;
            _merkleTree = merkleTree;
            _blockchainServiceFactory = blockchainServiceFactory;
            _workflowService = workflowService;
        }

        public Timestamp Timestamp { get; set; }

        public async Task<IActionResult> OnGetAsync(byte[] source)
        {
            if (source == null)
                return NotFound();

            Timestamp = await _context.Timestamps.SingleOrDefaultAsync(m => StructuralComparisons.StructuralEqualityComparer.Equals(m.Source, source));

            if (Timestamp == null)
                return NotFound();

            if (String.IsNullOrEmpty(Timestamp.Algorithm))
                Timestamp.Algorithm = "merkle-sha256.tc1";

            if (Timestamp.Source != null && Timestamp.Source.Length > 0)
            {
                var hash = _merkleTree.HashAlgorithm.HashOf(Timestamp.Source);
                var root = _merkleTree.ComputeRoot(hash, Timestamp.Receipt);

                if (Timestamp.WorkflowID > 0) {
                    var wf = _workflowService.Load<TimestampWorkflow>(Timestamp.WorkflowID);

                    if (String.IsNullOrEmpty(Timestamp.Blockchain))
                        Timestamp.Blockchain = wf.Proof.Blockchain;

                    var blockchainService = _blockchainServiceFactory.GetService(Timestamp.Blockchain);
                    var key = blockchainService.DerivationStrategy.GetKey(root);
                    var address = blockchainService.DerivationStrategy.StringifyAddress(key);

                    if (String.IsNullOrEmpty(Timestamp.Service))
                        Timestamp.Service = blockchainService.Repository.ServiceUrl;

                    ViewData["root"] = root.ToHex();
                    ViewData["rootMacth"] = (root.SequenceEqual(wf.Proof.MerkleRoot));
                    ViewData["address"] = address;
                    ViewData["confirmations"] = wf.Proof.Confirmations;
                    ViewData["addressLookupUrl"] = blockchainService.Repository.AddressLookupUrl(Timestamp.Blockchain, address);
                }
                
            }

            return Page();
        }
    }
}
