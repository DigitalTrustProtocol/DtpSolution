using DtpCore.Workflows;
using MediatR;
using System;
using System.Linq;
using DtpCore.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DtpCore.Interfaces;
using DtpCore.Builders;
using Microsoft.EntityFrameworkCore;
using DtpCore.Model.Configuration;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Commands;
using DtpStampCore.Interfaces;
using DtpPackageCore.Notifications;
using DtpPackageCore.Extensions;

namespace DtpPackageCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    [DisplayName("Create Trust Packages")]
    [Description("Create trust packages from new trusts received.")]
    public class CreateTrustPackageWorkflow : WorkflowContext
    {
        private readonly IMediator _mediator;
        private readonly ITrustDBService _trustDBService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateTrustPackageWorkflow> _logger;

        public CreateTrustPackageWorkflow(IMediator mediator, ITrustDBService trustDBService, IConfiguration configuration, ILogger<CreateTrustPackageWorkflow> logger)
        {
            _mediator = mediator;
            _trustDBService = trustDBService;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            var buildPackages = _trustDBService.GetBuildPackagesAsync().GetAwaiter().GetResult();

            foreach (var buildPackage in buildPackages)
            {
                var signedPackage = _mediator.SendAndWait(new BuildPackageCommand(buildPackage));
                if(signedPackage != null)
                    CombineLog(_logger, $"Package {signedPackage.Id.ToHex()}, scope: {signedPackage.Scopes}, created with {signedPackage.Claims.Count} claims.");

                _trustDBService.SaveChanges();
            }

            Wait(_configuration.TrustPackageWorkflowInterval()); // Never end the workflow
        }

    }

}
