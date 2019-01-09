using DtpCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Test the exception handling in API
    /// </summary>
    [Route("api/[controller]")]
    public class ThrowController : ApiController
    {
        [HttpGet]
        public object Get()
        {
            throw new InvalidOperationException("This is an unhandled exception");
        }
    }

    /// <summary>
    /// Test for heartbeat
    /// </summary>
    [Route("api/[controller]")]
    public class PingController : ApiController
    {
        [HttpGet]
        public ActionResult Get()
        {
            return ApiOk("OK");
        }

    }


}
