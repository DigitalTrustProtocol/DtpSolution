using System.Linq;
using DtpCore.Model;
using Microsoft.AspNetCore.Mvc;
using DtpCore.Controllers;
using DtpCore.Interfaces;
using System;
using DtpCore.Extensions;
using DtpCore.Repository;
using DtpCore.Model.Database;
using System.Collections.Generic;
using DtpCore.Services;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Handles trust related stuff.
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ClaimController : ApiController
    {
        public TrustDBService trustDBService;

        public ClaimController(TrustDBService trustDBService)
        {
            this.trustDBService = trustDBService;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="subjectId"></param>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public Claim GetOne(string issuerId, string subjectId, string scope, string type)
        {
            var query = this.trustDBService.GetActiveClaims();

            query = query.Where(p =>
                         p.Issuer.Id.Equals(issuerId) 
                         && p.Subject.Id.Equals(subjectId));

            if (!string.IsNullOrEmpty(scope))
            {
                scope = scope.ToLowerInvariant();
                query = query.Where(p => p.Scope == scope);
            }

            if (!string.IsNullOrEmpty(type))
            {
                type = type.ToLowerInvariant();
                query = query.Where(p => p.Type == type);
            }
            query = query.Take(100).OrderBy(p => p.DatabaseID);

            var result = query.Take(1).FirstOrDefault();

            return result;
        }




        //        /// <summary>
        //        /// Add a package to the Graph and database.
        //        /// </summary>
        //        /// <param name="package"></param>
        //        /// <returns></returns>
        //        [Produces("application/json")]
        //        [HttpPost]
        //        [Route("add")]
        //        public ActionResult Add([FromBody]Package package)
        //        {
        //            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Full);
        //            if (validationResult.ErrorsFound > 0)
        //                return ApiError(validationResult, null, "Validation failed");
        //            // Timestamp validation service disabled for the moment

        //            var result = _mediator.SendAndWait(new AddPackageCommand { Package = package });

        //            return ApiOk(result.LastOrDefault());
        //        }

        //        /// <summary>
        //        /// Create a trust, that is not added but returned for signing.
        //        /// </summary>
        //        /// <returns>A newly created trust object</returns>
        //        [HttpGet]
        //        [Route("build")]
        //        public ActionResult<Claim> BuildTrust(string issuer, string subject, string issuerScript = "", string type = PackageBuilder.BINARY_TRUST_DTP1, string attributes = "", string scope = "", string alias = "")
        //        {
        //            if (issuer == null || issuer.Length < 1)
        //                throw new ApplicationException("Missing issuer");

        //            if (subject == null || subject.Length < 1)
        //                throw new ApplicationException("Missing subject");

        //            if (string.IsNullOrEmpty(attributes))
        //                if (type == PackageBuilder.BINARY_TRUST_DTP1)
        //                    attributes = PackageBuilder.CreateBinaryTrustAttributes();

        //            var trustBuilder = new PackageBuilder(_serviceProvider);
        //            trustBuilder.AddClaim()
        //                .SetIssuer(issuer, issuerScript)
        //                .AddType(type, attributes)
        //                .AddSubject(subject)
        //                .BuildClaimID();

        //            return trustBuilder.CurrentClaim;
        //        }


        //        /// <summary>
        //        /// Build a package for the client to sign.
        //        /// </summary>
        //        /// <remarks>
        //        /// A client can add the trusts to a package and this function will build the package.
        //        /// The client however still needs to sign the package.
        //        /// </remarks>
        //        /// <param name="package"></param>
        //        /// <returns>A package with calculated id</returns>
        //        /// <response code="201">Returns the newly created item</response>
        //        /// <response code="400">If the package do not validate</response>    
        //        [HttpPost]
        //        [Route("build")]
        //        [ProducesResponseType(201)]
        //        [ProducesResponseType(400)]
        //        public ActionResult<Package> BuildTrust([FromBody]Package package)
        //        {
        //            var validationResult = _trustSchemaService.Validate(package, TrustSchemaValidationOptions.Basic);
        //            if (validationResult.ErrorsFound > 0)
        //                return ApiError(validationResult, null, "Validation failed");

        //            var trustBuilder = new PackageBuilder(_serviceProvider)
        //            {
        //                Package = package
        //            };
        //            trustBuilder.Build();

        //            return ApiOk(package);
        //        }

        //        [HttpGet]
        //        //[Route("get/{trustId}")]
        //        public ActionResult Get([FromRoute]byte[] claimId)
        //        {
        //            if (claimId == null || claimId.Length < 1)
        //                throw new ApplicationException("Missing trustId");

        //            var trust = _trustDBService.GetClaimById(claimId);

        //            return ApiOk(trust);
        //        }

        //        [HttpGet]
        //        [Route("get")]
        //        public ActionResult Get([FromQuery]string issuer, [FromQuery]string subject, [FromQuery]string type, [FromQuery]string scopevalue)
        //        {
        //            var query = new Claim
        //            {
        //                Issuer = new IssuerIdentity { Id = issuer },
        //                Subject = new SubjectIdentity { Id = subject },
        //                Type = type,
        //                Scope = scopevalue
        //            };

        //            var trust = _trustDBService.GetSimilarClaim(query);

        //            return ApiOk(trust);
        //        }
    }
}
