using Microsoft.AspNetCore.Mvc;
using DtpGraphCore.Model;
using DtpGraphCore.Interfaces;
using DtpServer.AspNetCore.MVC.Filters;
using DtpCore.Model;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Query the Graph
    /// </summary>
    [Route("api/graph/[controller]")]
    [ApiController]
    public class QueryController : ApiController
    {

        public IGraphQueryService SearchService { get; set; }
        //private IQueryRequestService _queryRequestService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="queryRequestService"></param>
        public QueryController(IGraphQueryService service)
        {
            SearchService = service;
            //_queryRequestService = queryRequestService;
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

        ///// <summary>
        ///// Query the graph on a single subject
        ///// </summary>
        ///// <param name="issuer"></param>
        ///// <param name="subject"></param>
        ///// <param name="flags"></param>
        ///// <returns>DtpGraphCore.Model.QueryContext</returns>
        //[HttpGet]
        //public ActionResult<QueryContext> Get(string issuer, string subject, QueryFlags flags = QueryFlags.LeafsOnly)
        //{
        //    var builder = new QueryRequestBuilder(null, PackageBuilder.BINARY_TRUST_DTP1);
        //    builder.Query.Flags = flags;
        //    builder.Add(issuer, subject);

        //    _queryRequestService.Verify(builder.Query);

        //    return ResolvePost(builder.Query);
        //}

        /// <summary>
        /// Query the graph on multiple subject
        /// </summary>
        /// <param name="query"></param>
        /// <returns>The result of the query</returns>
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ValidateModel(typeof(QueryRequest), typeof(IQueryRequestValidator))]
        [HttpPost]
        public ActionResult<QueryContext> ResolvePost([FromBody] QueryRequest query)
        {
            //_queryRequestService.Verify(query);

            var result = SearchService.Execute(query);

            return StatusCode(201, result);
        }
    }
}
