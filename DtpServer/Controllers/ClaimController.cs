using System.Linq;
using DtpCore.Model;
using Microsoft.AspNetCore.Mvc;
using DtpCore.Controllers;
using DtpCore.Extensions;
using System.Collections.Generic;
using DtpCore.Services;
using Microsoft.EntityFrameworkCore;
using DtpCore.Interfaces;
using DtpCore.Model.Database;
using System;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Handles trust related stuff.
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ClaimController : ApiController
    {
        public ITrustDBService trustDBService;

        public ClaimController(ITrustDBService trustDBService)
        {
            this.trustDBService = trustDBService;
        }



        /// <summary>
        /// Get one claim
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="subjectId"></param>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("inline")]
        public Claim GetOneInline(string issuerId, string subjectId, string scope, string type)
        {
            var time = DateTime.Now.ToUnixTime();
            var db = trustDBService.DBContext;
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

            var trusts = from c in query
                         join issuer in db.IdentityMetadata on c.Issuer.Id+c.Scope equals issuer.Id into issuerMeta
                         from issuerItem in issuerMeta.DefaultIfEmpty()
                         join subject in db.IdentityMetadata on c.Subject.Id + c.Scope equals subject.Id into subjectMeta
                         from subjectItem in subjectMeta.DefaultIfEmpty()
                         select new
                         {
                             claim = c,
                             issuerMeta = issuerItem,
                             subjectMeta = subjectItem

                         };


            var result = trusts.FirstOrDefault();
            if (result != null)
            {
                result.claim.Issuer.Meta = result.issuerMeta;
                result.claim.Subject.Meta = result.subjectMeta;
            }

            return result.claim;


            //query.GroupJoin(db.IdentityMetadata, c => c.Issuer.Id + c.Scope, m => m.Id, (c, m) => new { claim = c, meta = m });

            //var tet = query.SelectMany
            //(
            //    c => db.IdentityMetadata.Where(m => m.Id.Equals(c.Issuer.Id+c.Type)).DefaultIfEmpty(),
            //    (c, m) => new
            //    {
            //        claim = c,
            //        meta = m
            //    }
            //);

        }


        /// <summary>
        /// Get one claim
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

            query = query
                .Include(p => p.Issuer.Meta)
                .Include(p => p.Subject.Meta);

            var result = query.FirstOrDefault();

            return result;
        }

        //[HttpGet]
        //public IEnumerable<Claim> GetMany([FromBody]List<Claim> claims)
        //{
        //    var query = this.trustDBService.GetActiveClaims();

        //    query = query.Where(p =>
        //                 p.Issuer.Id.Equals(issuerId)
        //                 && p.Subject.Id.Equals(subjectId));

        //    if (!string.IsNullOrEmpty(scope))
        //    {
        //        scope = scope.ToLowerInvariant();
        //        query = query.Where(p => p.Scope == scope);
        //    }

        //    if (!string.IsNullOrEmpty(type))
        //    {
        //        type = type.ToLowerInvariant();
        //        query = query.Where(p => p.Type == type);
        //    }

        //    query = query
        //        .Include(p => p.Issuer.Meta)
        //        .Include(p => p.Subject.Meta);

        //    var result = query.FirstOrDefault();

        //    return result;
        //}

        /// <summary>
        /// Return a list of the latest create claims.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Route("latest")]
        public IEnumerable<Claim> GetLastest([FromQuery]int max = 10)
        {
            var query = this.trustDBService.GetActiveClaims();
            query = query
                .Include(p => p.Issuer.Meta)
                .Include(p => p.Subject.Meta)
                .OrderByDescending(p => p.Created);

            var result = query.Take(max).ToList();

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
