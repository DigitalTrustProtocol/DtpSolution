using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DtpCore.Model;
using DtpCore.Collections.Generic;
using DtpCore.Interfaces;
using MediatR;
using DtpPackageCore.Commands;
using DtpStampCore.Interfaces;

namespace DtpServer.Pages.Packages
{
    public class DetailsModel : PageModel
    {
        private readonly IMediator mediator;
        private readonly ITrustDBService _trustDBService;
        private readonly ITimestampService _timestampService;

        public DetailsModel(IMediator mediator, ITrustDBService trustDBService, ITimestampService timestampService)
        {
            this.mediator = mediator;
            _trustDBService = trustDBService;
            _timestampService = timestampService;
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
                return NotFound();

            if (string.IsNullOrEmpty(Package.Scopes))
                Package.Scopes = "Global";

            var count = 0;            
            foreach (var timestamp in Package.Timestamps)
            {
                if(timestamp.Proof == null || string.IsNullOrEmpty(timestamp.Proof.Address))
                    timestamp.Proof = _timestampService.GetProof(timestamp);

                if (count++ > 3) // Minimize attack vector off having to many timestamps to check
                    break;
            }

            //var query = _trustDBService.DBContext.ClaimPackageRelationships.Where(p => p.PackageID == id).Include(p => p.Claim).OrderBy(p => p.Claim.Created).Select(p => p.Claim);
            //Claims = await PaginatedList<Claim>.CreateAsync(query, 1, 0); // PageSize = 0 is unlimited

            return Page();
        }
    }
}
