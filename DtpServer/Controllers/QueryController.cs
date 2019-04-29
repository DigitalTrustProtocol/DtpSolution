using Microsoft.AspNetCore.Mvc;
using System;
using DtpCore.Builders;
using DtpGraphCore.Model;
using DtpGraphCore.Interfaces;
using DtpCore.Controllers;
using DtpGraphCore.Builders;
using DtpGraphCore.Enumerations;
using System.Threading.Tasks;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Query the Graph
    /// </summary>
    [Route("api/graph/[controller]")]
    public class QueryController : ApiController
    {

        public IGraphQueryService SearchService { get; set; }
        private IQueryRequestService _queryRequestService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="queryRequestService"></param>
        /// <param name="serviceProvider"></param>
        public QueryController(IGraphQueryService service, IQueryRequestService queryRequestService)
        {
            SearchService = service;
            _queryRequestService = queryRequestService;
        }

        //        public IHttpActionResult Get(string issuer, string subject, string subjectType = "", string? scope, bool? trust, bool? confirm, bool? rating)

        // GET api/
        //[HttpGet]
        //public ActionResult Get(string issuer, string subject, bool trust = true)
        //{
        //    var builder = new QueryRequestBuilder("", TrustBuilder.BINARYTRUST_TC1);
        //    var sub = new Subject
        //    {
        //        Address = Convert.FromBase64String(subject),
        //        Type = ""
        //    };
        //    builder.Add(Convert.FromBase64String(issuer), sub);

        //    _queryRequestService.Verify(builder.Query);

        //    return ResolvePost(builder.Query);
        //}

        /// <summary>
        /// Query the graph on a single subject
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="subject"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(string issuer, string subject, QueryFlags flags = QueryFlags.LeafsOnly)
        {
            var builder = new QueryRequestBuilder(null, PackageBuilder.BINARY_TRUST_DTP1);
            builder.Query.Flags = flags;
            builder.Add(issuer, subject);

            _queryRequestService.Verify(builder.Query);

            return ResolvePost(builder.Query);
        }

        /// <summary>
        /// Query the graph on multiple subject
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ResolvePost([FromBody]QueryRequest query)
        {
            _queryRequestService.Verify(query);

            var result = SearchService.Execute(query);



            return StatusCode(201, result);
        }
    }
}
