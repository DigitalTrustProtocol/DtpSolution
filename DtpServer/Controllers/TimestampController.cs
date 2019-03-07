using Microsoft.AspNetCore.Mvc;
using DtpCore.Controllers;
using MediatR;
using DtpStampCore.Commands;
using System.Threading.Tasks;
using DtpCore.Repository;

namespace DtpServer.Controllers
{
    [Route("api/[controller]")]
    public class TimestampController : ApiController
    {

        private IMediator _mediator;
        private TrustDBContext _db;

        public TimestampController(IMediator mediator, TrustDBContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpPost] 
        [Route("api/[controller]")]
        public async Task<IActionResult> Add([FromBody]byte[] source)
        {
            var result = await _mediator.Send(new CreateTimestampCommand(source));
            await _db.SaveChangesAsync(); // Call save here, as CreateTimestampCommand do not Save to DB
            return StatusCode(201, result);
        }


        [HttpGet]
        [Route("api/[controller]/{source}")]
        public async Task<IActionResult> Get(string blockchain, byte[] source,[FromQuery]bool includeProof = true)
        {
            var result = await _mediator.Send(new GetTimestampCommand(source, includeProof));
            return StatusCode(200, result);
        }
    }
}