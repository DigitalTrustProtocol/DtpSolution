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
using DtpCore.Factories;
using Microsoft.Extensions.Configuration;

namespace DtpServer.Pages.Timestamps
{
    public class DetailsModel : PageModel
    {
        private readonly TrustDBContext _context;
        private readonly IMerkleTree _merkleTree;
        private readonly IBlockchainServiceFactory _blockchainServiceFactory;
        private readonly IWorkflowService _workflowService;
        private readonly IConfiguration _configuration;

        public DetailsModel(TrustDBContext context, IMerkleTree merkleTree, IBlockchainServiceFactory blockchainServiceFactory, IWorkflowService workflowService, IConfiguration configuration)
        {
            _context = context;
            _merkleTree = merkleTree;
            _blockchainServiceFactory = blockchainServiceFactory;
            _workflowService = workflowService;
            _configuration = configuration;
        }

        public Timestamp Timestamp { get; set; }

        public IActionResult OnGet(byte[] source)
        {
            if (source == null)
                return NotFound();

            Timestamp = _context.Timestamps.SingleOrDefault(m => m.Source == source);

            if (Timestamp == null)
                return NotFound();

            if (string.IsNullOrEmpty(Timestamp.Type))
                Timestamp.Type = Timestamp.DEFAULT_TYPE;

            if (Timestamp.Source == null && Timestamp.Source.Length == 0)
                return Page();

            //var proof = _context.Proofs.FirstOrDefault(p => p.DatabaseID == Timestamp.BlockchainProof_db_ID);
            //if (proof == null)
            //    return Page();

            //if (proof.Blockchain == null)
            //    proof.Blockchain = _configuration.Blockchain();

            //var hash = _merkleTree.HashAlgorithm.HashOf(Timestamp.Source);
            //var root = _merkleTree.ComputeRoot(hash, Timestamp.Path);

            //Timestamp.Blockchain = proof.Blockchain;

            //var blockchainService = _blockchainServiceFactory.GetService(Timestamp.Blockchain);
            //var key = blockchainService.DerivationStrategy.GetKey(root);
            //var address = blockchainService.DerivationStrategy.GetAddress(key);

            //if (String.IsNullOrEmpty(Timestamp.Service))
            //    Timestamp.Service = blockchainService.Repository.ServiceUrl;

            //ViewData["root"] = root.ToHex();
            //ViewData["rootMacth"] = root.SequenceEqual(proof.MerkleRoot);
            //ViewData["address"] = proof.Address;
            //ViewData["confirmations"] = proof.Confirmations;
            //ViewData["addressLookupUrl"] = blockchainService.Repository.AddressLookupUrl(Timestamp.Blockchain, address);

            return Page();
        }
    }
}
