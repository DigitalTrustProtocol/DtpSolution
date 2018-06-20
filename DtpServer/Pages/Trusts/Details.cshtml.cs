using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using DtpCore.Interfaces;

namespace DtpServer.Pages.Trusts
{
    public class DetailsModel : PageModel
    {
        private readonly ITrustDBService _trustDBService;

        public DetailsModel(ITrustDBService trustDBService)
        {
            _trustDBService = trustDBService;
        }

        public Trust Trust { get;set; }

        public void OnGet(byte[] id)
        {
            Trust = _trustDBService.GetTrustById(id);
        }
    }
}
