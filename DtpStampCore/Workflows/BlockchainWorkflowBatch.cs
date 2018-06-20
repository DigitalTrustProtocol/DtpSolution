using TrustStampCore.Repository;
using TrustStampCore.Service;
using TrustStampCore.Extensions;
using System;

namespace TruststampCore.Workflows
{
    public class BlockchainWorkflowBatch : WorkflowBatch
    {

        public IBlockchainRepository BlockchainRepository { get; set; }
        public byte[] Root { get; set; }

        public bool EnsureRepository()
        {
            var blockchainRepositoryName = App.Config["blockchainprovider"].ToStringValue("blockr");
            var BlockchainRepository = BlockchainFactory.GetRepository(blockchainRepositoryName, BlockchainFactory.GetBitcoinNetwork());
            if (BlockchainRepository == null)
            {
                WriteLog("No blockchain provider found"); // No comment!
                return false;
            }
            return true;
        }

        public bool EnsureRoot()
        {
            var hash = (byte[])CurrentBatch["root"];
            if (hash.Length == 0)
            {
                WriteLog("No root to timestamp!");
                return false;
            }
            return true;
        }

        public bool EnsureDependencies()
        {
            if (!EnsureRoot())
                return false;

            if (!EnsureRepository())
                return false;

            return true;
        }

        public override bool Initialize()
        {
            if (!base.Initialize())
                return false;
            
             return EnsureDependencies(); 
        }
    }
}
