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

namespace DtpPackageCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    [DisplayName("Create Trust Packages")]
    [Description("Create trust packages from new trusts received.")]
    public class CreateTrustPackageWorkflow : WorkflowContext
    {
        private IMediator _mediator;
        private IConfiguration _configuration;
        private ILogger<CreateTrustPackageWorkflow> _logger;


        public CreateTrustPackageWorkflow(IMediator mediator, IConfiguration configuration, ILogger<CreateTrustPackageWorkflow> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        public override void Execute()
        {
            var package = _mediator.SendAndWait(new AddNewTrustPackageCommand());
            if (package == null)
            {
                return;
            }

            _mediator.Publish(new TrustPackageCreatedNotification(package));

            CombineLog(_logger, $"Package ({package.Id}) created with {package.Trusts.Count} trusts.");

            Wait(_configuration.TrustPackageWorkflowInterval()); // Never end the workflow
        }

    }

}
