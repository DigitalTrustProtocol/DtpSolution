using System;
using System.Linq;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpStampCore.Interfaces;
using DtpCore.Extensions;
using System.Collections;
using System.Collections.Generic;
using DtpCore.Factories;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;

namespace DtpStampCore.Services
{
    public class TimestampService : ITimestampService
    {
        private ITimestampSynchronizationService _timestampSynchronizationService;
        private ITrustDBService _trustDBService;
        private IBlockchainProofFactory _timestampProofFactory;
        private readonly IConfiguration _configuration;

        public TimestampService(ITimestampSynchronizationService timestampSynchronizationService, ITrustDBService trustDBService, IBlockchainProofFactory timestampProofFactory, IConfiguration configuration)
        {
            _timestampSynchronizationService = timestampSynchronizationService;
            _trustDBService = trustDBService;
            _timestampProofFactory = timestampProofFactory;
            _configuration = configuration;
        }

        public IQueryable<Timestamp> Timestamps
        {
            get
            {
                return _trustDBService.Timestamps;
            }
        }
        
        public IList<Timestamp> FillIn(IList<Timestamp> timestamps, byte[] source)
        {
            var list = timestamps ?? new List<Timestamp>();

            var item = Create(source);
            list.Add(item);
            
            return list;
        }

        public Timestamp Create(byte[] source)
        {
            var timestamp = new Timestamp
            {
                WorkflowID = _timestampSynchronizationService.CurrentWorkflowID,
                Algorithm = MerkleStrategyFactory.DOUBLE256_MERKLE_DTP1,
                Blockchain = _configuration.Blockchain(),
                Source = source,
                Registered = DateTime.Now.ToUnixTime()
            };
            return timestamp;
        }

        public Timestamp Add(byte[] source, bool save = true)
        {
            var proof = Get(source);
            if(proof == null)
            {
                proof = Create(source);
                _trustDBService.DBContext.Timestamps.Add(proof);
                if(save)
                    _trustDBService.DBContext.SaveChanges();
            }
            return proof;
        }

        public Timestamp Get(byte[] source)
        {
            var proof = _trustDBService.Timestamps.FirstOrDefault(p => StructuralComparisons.StructuralEqualityComparer.Equals(p.Source, source));
            return proof;
        }

        public BlockchainProof GetBlockchainTimestamp(byte[] source)
        {
            var entity = Get(source);
            
            return _timestampProofFactory.Create(entity);
        }

        
    }
}
