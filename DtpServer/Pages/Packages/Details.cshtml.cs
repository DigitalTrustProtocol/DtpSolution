using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Collections.Generic;
using DtpCore.Interfaces;
using MediatR;
using DtpCore.Commands.Packages;

namespace DtpServer.Pages.Packages
{
    public class DetailsModel : PageModel
    {
        private readonly IMediator mediator;
        private readonly ITrustDBService _trustDBService;

        public DetailsModel(IMediator mediator, ITrustDBService trustDBService)
        {
            this.mediator = mediator;
            _trustDBService = trustDBService;
        }

        public Package Package { get; set; }
        public PaginatedList<Claim> Claims { get; set; }

        /// <summary>
        /// Get a package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            Package = await mediator.Send(new GetPackageCommand { DatabaseID = (int)id });

            if (Package == null)
            {
                return NotFound();
            }

            var query = _trustDBService.DBContext.ClaimPackageRelationships.Where(p => p.PackageID == id).Include(p => p.Claim).OrderBy(p => p.Claim.Created).Select(p => p.Claim);
            Claims = await PaginatedList<Claim>.CreateAsync(query, 1, 0); // PageSize = 0 is unlimited

            return Page();
        }
    }
}
