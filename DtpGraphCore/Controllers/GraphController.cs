using Microsoft.AspNetCore.Mvc;
using DtpCore.Controllers;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Controllers
{
    [Route("api/[controller]")]
    public class GraphController : ApiController
    {
        public IGraphExportService ExportService { get; set; }

        public GraphController(IGraphExportService service)
        {
            ExportService = service;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var result = ExportService.GetFullGraph();

            return ApiOk(result);
        }
    }
}
