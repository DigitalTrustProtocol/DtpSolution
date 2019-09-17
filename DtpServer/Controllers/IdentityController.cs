using DtpCore.Controllers;
using DtpCore.Model;
using DtpCore.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DtpServer.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class IdentityController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public TrustDBContext trustDBContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trustDBContext"></param>
        public IdentityController(TrustDBContext trustDBContext)
        {
            this.trustDBContext = trustDBContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/id")]
        public string[] Search(string term, string scope, string type)
        {

            var query = this.trustDBContext.Claims.Where(p => p.Issuer.Id.StartsWith(term) || p.Subject.Id.StartsWith(term));
            if(!string.IsNullOrEmpty(scope))
            {
                scope = scope.ToLowerInvariant();
                query = query.Where(p => p.Scope == scope);
            }

            if (!string.IsNullOrEmpty(type))
            {
                type = type.ToLowerInvariant();
                query = query.Where(p => p.Type == type);
            }
            query = query.OrderBy(p => p.DatabaseID).Take(100);

            SortedSet<string> hs = new SortedSet<string>();
            foreach (var claim in query)
            {
                if (claim.Issuer.Id.StartsWith(term, StringComparison.InvariantCulture))
                    hs.Add(claim.Issuer.Id);

                if (claim.Subject.Id.StartsWith(term, StringComparison.InvariantCulture))
                    hs.Add(claim.Subject.Id);
            }

            return hs.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("metadata")]
        public IdentityMetadata GetMetadata(string id, string scope = "")
        {
            var combinedId = id + scope.ToLowerInvariant();
            var query = trustDBContext.IdentityMetadata.Where(p => p.Id == combinedId);
            return query.FirstOrDefault();
        }
    }
}
