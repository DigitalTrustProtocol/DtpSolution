using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using DtpCore.Builders;
using DtpGraphCore.Model;
using DtpGraphCore.Interfaces;
using DtpCore.Controllers;
using DtpGraphCore.Builders;
using DtpCore.Model;
using DtpGraphCore.Enumerations;
using DtpCore.Interfaces;
using System.Text;
using DtpCore.Strategy;

namespace DtpGraphCore.Controllers
{
    [Route("api/graph/[controller]")]
    public class QueryController : ApiController
    {

        public IGraphQueryService SearchService { get; set; }
        private IQueryRequestService _queryRequestService;
        public IServiceProvider ServiceProvider { get; set; }


        public QueryController(IGraphQueryService service, IQueryRequestService queryRequestService, IServiceProvider serviceProvider)
        {
            SearchService = service;
            _queryRequestService = queryRequestService;
            ServiceProvider = serviceProvider;
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

        [HttpGet]
        public ActionResult Get(byte[] issuer, byte[] subject, QueryFlags flags = QueryFlags.LeafsOnly)
        {
            var builder = new QueryRequestBuilder(null, TrustBuilder.BINARYTRUST_TC1);
            builder.Query.Flags = flags;
            builder.Add(issuer, subject);

            _queryRequestService.Verify(builder.Query);

            return ResolvePost(builder.Query);
        }

        // Post api/
        [HttpPost]
        public ActionResult ResolvePost([FromBody]QueryRequest query)
        {
            _queryRequestService.Verify(query);

            var result = SearchService.Execute(query);

            return ApiOk(result);
        }
    }
}
