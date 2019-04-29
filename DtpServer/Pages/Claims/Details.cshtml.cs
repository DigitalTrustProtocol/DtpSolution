using Microsoft.AspNetCore.Mvc.RazorPages;
using DtpCore.Model;
using DtpCore.Interfaces;

namespace DtpServer.Pages.Claims
{
    public class DetailsModel : PageModel
    {
        private readonly ITrustDBService _trustDBService;

        public DetailsModel(ITrustDBService trustDBService)
        {
            _trustDBService = trustDBService;
        }

        public Claim Claim { get;set; }

        public void OnGet(byte[] id)
        {
            Claim = _trustDBService.GetClaimById(id);

        }
    }
}
