using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model;
using DtpCore.Repository;
using System.Linq;
using DtpCore.Model.Database;
using Microsoft.Extensions.Configuration;
using MediatR;
using DtpCore.Extensions;
using DtpPackageCore.Commands;
using Microsoft.AspNetCore.Mvc;
using DtpCore.Interfaces;

namespace DtpServer.Pages.Packages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly TrustDBContext _context;
        private readonly ITimestampProofValidator _timestampProofValidator;
        private readonly IConfiguration _configuration;

        public IndexModel(IMediator mediator, TrustDBContext context, ITimestampProofValidator timestampProofValidator, IConfiguration configuration)
        {
            _mediator = mediator;
            _context = context;
            _timestampProofValidator = timestampProofValidator;
            _configuration = configuration;
        }

        public IList<Package> BuildPackages { get; set; }
        public IList<Package> Packages { get;set; }

        public bool Admin { get; set; }
        public string CommandResult { get; set; }

        /// <summary>
        /// Load data
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync(int? id = null, string action = null)
        {
            Admin = _configuration.IsAdminEnabled(Admin);
            BuildPackages = await _context.Packages.AsNoTracking()
                .Where(p => p.State == PackageStateType.Building)
                .ToListAsync();

            foreach (var package in BuildPackages)
            {
                if (string.IsNullOrEmpty(package.Scopes))
                    package.Scopes = "Global";
            }

            Packages = await _context.Packages.AsNoTracking().Where(p => p.State != PackageStateType.Building).ToListAsync();
            foreach (var package in Packages)
            {
                if (string.IsNullOrEmpty(package.Scopes))
                    package.Scopes = "Global";
            }


        }

        /// <summary>
        /// Verify package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnGetVerifyAsync(int? id)
        {
            await OnGetAsync();

            if (id == null)
                return;

            if (Admin && id != null)
            {
                // Get full package
                var package = await _mediator.Send(new GetPackageCommand { DatabaseID = (int)id });

                if (package == null)
                    return;

                if (package.Timestamps.Count == 0)
                {
                    CommandResult = "Missing timestamp object";
                    return;
                }

                if (!_timestampProofValidator.Validate(package.Timestamps[0], out IList<string> errors))
                    CommandResult = string.Join(", ", errors);
                else
                    CommandResult = $"Package {package.Id.ToHex()} is valid!";
            }
        }


        /// <summary>
        /// Re publish package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task OnGetPublishAsync(int? id)
        {
            await OnGetAsync();

            if (id == null)
                return;

            if (Admin && id != null)
            {
                var package = Packages.FirstOrDefault(p => p.DatabaseID == id);
                if (package != null)
                {
                    await _mediator.Send(new PublishPackageCommand(package));
                    CommandResult = $"Package ID {package.Id.ToHex()} published";
                }

            }
        }


    }
}
