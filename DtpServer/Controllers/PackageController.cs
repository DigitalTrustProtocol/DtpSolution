using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DtpCore.Model;
using DtpCore.Controllers;
using DtpServer.AspNetCore.MVC.Filters;
using DtpCore.Interfaces;
using MediatR;
using DtpPackageCore.Commands;
using DtpCore.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DtpCore.Model.Database;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Handles packages 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ApiController
    {
        private IMediator _mediator;

        private IPackageSchemaValidator _trustSchemaService;
        private ITrustDBService _trustDBService;
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public PackageController(IMediator mediator, IPackageSchemaValidator trustSchemaService, ITrustDBService trustDBService, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _trustSchemaService = trustSchemaService;
            _trustDBService = trustDBService;
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// Add a package to the server
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        // POST: api/Packages
        [ValidatePackage]
        [HttpPost]
        public async Task<IActionResult> PostPackage([FromBody] Package package)
        {
            var result = await _mediator.Send(new AddPackageCommand(package));
            return StatusCode(201, result);
        }


        /// <summary>
        /// List all packages
        /// </summary>
        /// <returns></returns>
        // GET: api/Packages
        [Route("info")]
        [HttpGet]
        public PackageInfoCollection GetPackageInfoCollection([FromQuery]long from = 0)
        {
            var query = _trustDBService.DBContext.Packages.AsNoTracking(); // Makes read-only queries faster
            
            // The package need to be signed and not replaced!
            query = query.Where(p => (p.State & PackageStateType.Signed) > 0 && (p.State & PackageStateType.Replaced) == 0);
            query = query.Where(p => p.File != null);

            if (from > 0)
                query = query.Where(p => p.Created >= from);

            query = query.OrderByDescending(p => p.Created);

            var result = new PackageInfoCollection();

            foreach (var entity in query)
            {
                result.Packages.Add(new PackageInfo { Id = entity.Id, File = entity.File });
            }
           
            return result;
        }

        ///// <summary>
        ///// Get a package by ID
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //// GET: api/Packages/5
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetPackage([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var package = await _context.Packages.FindAsync(id);

        //    if (package == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(package);
        //}

        //private bool PackageExists(int id)
        //{
        //    return _context.Packages.Any(e => e.DatabaseID == id);
        //}
    }
}