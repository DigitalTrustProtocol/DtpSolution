using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Model.Configuration;
using DtpCore.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DtpStampCore.Commands
{
    public class CurrentBlockchainProofQueryHandler : IRequestHandler<CurrentBlockchainProofQuery, BlockchainProof>
    {

        private IMediator _mediator;
        private TrustDBContext _db;
        private readonly ILogger<CurrentBlockchainProofQueryHandler> _logger;

        public CurrentBlockchainProofQueryHandler(IMediator mediator, TrustDBContext db, ILogger<CurrentBlockchainProofQueryHandler> logger)
        {
            _mediator = mediator;
            _db = db;
            _logger = logger;
        }

        public Task<BlockchainProof> Handle(CurrentBlockchainProofQuery request, CancellationToken cancellationToken)
        {
            var proof = _db.Proofs
                .Where(p => p.Status == ProofStatusType.New.ToString())
                .OrderBy(p => p.DatabaseID)
                .Include(p => p.Timestamps)
                .FirstOrDefault();

            if (proof == null)
            {
                proof = _mediator.SendAndWait(new AddNewBlockchainProofCommand());
            }
                
            return Task.FromResult(proof);
        }

        //protected override BlockchainProof Handle(CreateNewBlockchainProofCommand request)
        //{
        //    var proof = _db.Proofs.Where(p => p.MerkleRoot == null).OrderByDescending(p => p.DatabaseID).FirstOrDefault();
        //    if (proof == null)
        //    {
        //        proof = _mediator.Send(new CreateNewBlockchainProofCommand()).GetAwaiter().GetResult();
        //    }

        //    return proof;
        //}

        //private IConfiguration _configuration;
        //private ITrustDBService _trustDBService;
        //private IDerivationStrategyFactory _derivationStrategyFactory;
        //private ITrustPackageService _trustPackageService;
        //private ITimestampService _timestampService;
        //private readonly IServiceProvider _serviceProvider;


        //public override BlockchainProof Handle(GetCurrentBlockchainProofCommand request)
        //{




        //    return null;
        //    //var trusts = GetTrusts();
        //    //TrustBuilder _builder = _serviceProvider.GetRequiredService<TrustBuilder>();
        //    //_builder.AddTrust(trusts);
        //    //_builder.OrderTrust(); // Order trust ny ID before package ID calculation. 
        //    //if (_builder.Package.Trusts.Count == 0)
        //    //    // No trusts found, exit
        //    //    return null;

        //    //SignPackage(_builder);

        //    //_builder.Package.Timestamps = _timestampService.FillIn(_builder.Package.Timestamps, _builder.Package.Id);

        //    //_trustDBService.DBContext.Packages.Add(_builder.Package);
        //    //await _trustDBService.DBContext.SaveChangesAsync();

        //    //return _builder.Package;
        //}


    }
}
