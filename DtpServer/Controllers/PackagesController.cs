using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using Sieve.Models;
using Sieve.Services;
using DtpCore.Controllers;
using System.Net;
using DtpServer.AspNetCore.MVC.Filters;

namespace DtpServer.Controllers
{
    /// <summary>
    /// Handles packages 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ApiController
    {
        private readonly TrustDBContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public PackagesController(TrustDBContext context)
        {
            _context = context;
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
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return StatusCode(201);
            //return Created();
            //return CreatedAtAction("GetPackage", new { id = package.DatabaseID }, package);
        }

        //private bool PackageExists(int id)
        //{
        //    return _context.Packages.Any(e => e.DatabaseID == id);
        //}
    }
}