using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DtpCore.Controllers;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpStampCore.Interfaces;

namespace DtpStampCore.Controllers
{
    public class TimestampController : ApiController
    {

        private ITimestampService _proofService;

        public TimestampController(ITimestampService proofService)
        {
            _proofService = proofService;
        }
        
        [HttpPost] // string blockchain, 
        [Route("api/[controller]")]
        public ActionResult Add([FromBody]byte[] source)
        {
            return ApiOk(_proofService.Add(source));
        }

        [HttpGet] // string blockchain, 
        //[Route("api/{blockchain}/[controller]/add/{source}")]
        [Route("api/[controller]/add/{source}")]
        public ActionResult AddString(string blockchain, string source)
        {
            if (String.IsNullOrWhiteSpace(source))
                throw new ApplicationException("Source cannot be empty.");

            var data = Encoding.UTF8.GetBytes(source);
            return ApiOk(_proofService.Add(data));
        }



        // GET api/
        [HttpGet]
        [Route("api/[controller]/{source}")]
        public ActionResult Get(string blockchain, byte[] source)
        {
            return ApiOk(_proofService.GetBlockchainTimestamp(source));
        }


        [HttpGet]
        [Route("api/[controller]/query")]
        public ActionResult Load(String sort, String order, String search, Int32 limit, Int32 offset, String ExtraParam)
        {
            // Get entity fieldnames
            List<String> columnNames = typeof(Timestamp).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

            // Create a seperate list for searchable field names   
            List<String> searchFields = new List<String>(columnNames);

            // Exclude field Iso2 for filtering 
            //searchFields.Remove("ISO2");

            // Perform filtering
            IQueryable items = _proofService.Timestamps.Search(search, searchFields);

            // Sort the filtered items and apply paging
            return Ok(items.Filter(columnNames, sort, order, limit, offset));
        }
    }
}

