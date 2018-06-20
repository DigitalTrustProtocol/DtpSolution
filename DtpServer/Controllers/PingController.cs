using Microsoft.AspNetCore.Mvc;
using System;

namespace DtpCore.Controllers
{
    [Route("api/[controller]")]
    public class ThrowController : ApiController
    {
        [HttpGet]
        public object Get()
        {
            throw new InvalidOperationException("This is an unhandled exception");
        }
    }

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
