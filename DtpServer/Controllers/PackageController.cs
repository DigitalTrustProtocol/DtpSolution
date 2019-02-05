using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DtpCore.Model;
using DtpCore.Controllers;
using DtpServer.AspNetCore.MVC.Filters;
using DtpCore.Interfaces;
using MediatR;
using DtpCore.Extensions;
using DtpCore.Commands.Packages;

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
            //_context.Packages.Add(package);
            //await _context.SaveChangesAsync();

            var result = await _mediator.Send(new AddPackageCommand { Package = package });

            return StatusCode(201, result);
            //return Created();
            //return CreatedAtAction("GetPackage", new { id = package.DatabaseID }, package);
        }


        ///// <summary>
        ///// List all packages
        ///// </summary>
        ///// <returns></returns>
        //// GET: api/Packages
        //[HttpGet]
        //public IEnumerable<Package> GetPackages([FromServices]SieveProcessor sieveProcessor, SieveModel sieveModel)
        //{
        //    var packages = _context.Packages.AsNoTracking(); // Makes read-only queries faster
        //    packages = sieveProcessor.Apply(sieveModel, packages); // Returns `result` after applying the sort/filter/page query in `SieveModel` to it
        //    return packages;
        //}

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