using System.Linq;
using DtpCore.Model;
using Microsoft.AspNetCore.Mvc;
using DtpGraphCore.Interfaces;
using DtpCore.Controllers;
using DtpCore.Interfaces;
using DtpStampCore.Interfaces;
using System;
using DtpCore.Builders;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using System.Collections.Generic;
using MediatR;
using DtpCore.Commands;

namespace DtpGraphCore.Controllers
{
    [Route("api/[controller]")]
    public class TrustController : ApiController
    {
        private IMediator _mediator;

        private IGraphTrustService _graphTrustService;
        private ITrustSchemaService _trustSchemaService;
        private ITrustDBService _trustDBService;
        private IBlockchainServiceFactory _blockchainServiceFactory;
        private IServiceProvider _serviceProvider;



        public TrustController(IMediator mediator, IGraphTrustService graphTrustService, ITrustSchemaService trustSchemaService, ITrustDBService trustDBService, IBlockchainServiceFactory blockchainServiceFactory, IServiceProvider serviceProvider)
        {
            _mediator = mediator;

            _graphTrustService = graphTrustService;
            _trustSchemaService = trustSchemaService;
            _trustDBService = trustDBService;
            _blockchainServiceFactory = blockchainServiceFactory;
            _serviceProvider = serviceProvider;
        }

  
        /// <summary>
        /// Add a package to the Graph and database.
        /// If the package is not timestamped, then it will be at a time interval.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        [Produces("application/json")]
        [HttpPost]
        [Route("add")]
        public ActionResult Add([FromBody]Package package)
        {
            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Full);
            if (validationResult.ErrorsFound > 0)
                return ApiError(validationResult, null, "Validation failed");
            // Timestamp validation service disabled for the moment

            // Do not add external packages, as this system do not support this in data structure.
            //if ((package.Id != null && package.Id.Length > 0))
            //{
            //    if (_trustDBService.DBContext.Packages.Any(f => f.Id == package.Id))
            //        throw new ApplicationException("Package already exist");
            //}

            foreach (var trust in package.Trusts)
            {
                AddTrust(trust);
            }

            _trustDBService.DBContext.SaveChanges();

            return ApiOk("Package added");
        }


        private void AddTrust(Trust trust)
        {
            if (_trustDBService.TrustExist(trust.Id))
                return; // TODO: Ignore the same trust for now.
                //throw new ApplicationException("Trust already exist");

            var dbTrust = _trustDBService.GetSimilarTrust(trust);
            if (dbTrust != null)
            {
                // TODO: Needs to verfify with Timestamp if exist, for deciding action!
                // The trick is to compare "created" in order to awoid old trust being replayed.
                // For now, we just remove the old trust
                if (dbTrust.Created > trust.Created)
                    throw new ApplicationException("Cannot add old trust, newer trust exist.");

                // Check if everything is the same except Created date, then what?
                //trust.Activate 
                //trust.Expire
                //trust.Cost
                //trust.Claim
                //trust.Note

                dbTrust.Replaced = true;
                _trustDBService.Update(dbTrust);
                _graphTrustService.Remove(trust);
            }


            var timestamp = _mediator.SendAndWait(new CreateTimestampCommand { Source = trust.Id });
            trust.Timestamps = trust.Timestamps ?? new List<Timestamp>();
            trust.Timestamps.Add(timestamp);

            // Timestamp objects gets added to the Timestamp table as well!
            _trustDBService.Add(trust);   

            var time = DateTime.Now.ToUnixTime();
            if ((trust.Expire  == 0 || trust.Expire > time) 
                && (trust.Activate == 0 || trust.Activate <= time)) 
                _graphTrustService.Add(trust);    // Add to Graph
        }


        /// <summary>
        /// Create a trust, that is not added but returned for signing.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("build")]
        public ActionResult BuildTrust(string issuer, string subject, string issuerScript = "", string type = TrustBuilder.BINARY_TRUST_DTP1, string attributes = "", string scope = "", string alias = "")
        {
            if (issuer == null || issuer.Length < 1)
                throw new ApplicationException("Missing issuer");

            if (subject == null || subject.Length < 1)
                throw new ApplicationException("Missing subject");

            if (string.IsNullOrEmpty(attributes))
                if (type == TrustBuilder.BINARY_TRUST_DTP1)
                    attributes = TrustBuilder.CreateBinaryTrustAttributes();

            var trustBuilder = new TrustBuilder(_serviceProvider);
            trustBuilder.AddTrust()
                .SetIssuer(issuer, issuerScript)
                .AddType(type, attributes)
                .AddSubject(subject)
                .BuildTrustID();

            return ApiOk(trustBuilder.CurrentTrust);
        }


        /// <summary>
        /// Build a trust for the client to sign.
        /// </summary>
        /// <param name="trust"></param>
        /// <returns>trust</returns>
        [Produces("application/json")]
        [HttpPost]
        [Route("build")]
        public ActionResult BuildTrust([FromBody]Package package)
        {
            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Basic);
            if (validationResult.ErrorsFound > 0)
                return ApiError(validationResult, null, "Validation failed");

            var trustBuilder = new TrustBuilder(_serviceProvider)
            {
                Package = package
            };
            trustBuilder.Build();

            return ApiOk(package);
        }

        [HttpGet]
        //[Route("get/{trustId}")]
        public ActionResult Get([FromRoute]byte[] trustId)
        {
            if (trustId == null || trustId.Length < 1)
                throw new ApplicationException("Missing trustId");

            var trust = _trustDBService.GetTrustById(trustId);

            return ApiOk(trust);
        }

        [HttpGet]
        [Route("get")]
        public ActionResult Get([FromQuery]string issuer, [FromQuery]string subject, [FromQuery]string type, [FromQuery]string scopevalue)
        {
            var query = new Trust
            {
                Issuer = new IssuerIdentity { Address = issuer },
                Subject = new SubjectIdentity { Address = subject },
                Type = type,
                Scope = new Scope { Value = scopevalue }
            };

            var trust = _trustDBService.GetSimilarTrust(query);

            return ApiOk(trust);
        }
    }
}
