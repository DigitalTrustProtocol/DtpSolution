using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpPackageCore.Commands;
using DtpPackageCore.Interfaces;
using DtpPackageCore.Model;
using Ipfs;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore
{
    public class PubSubMessageController
    {

        //public ICoreApi Ipfs { get; set; }
        private IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IServerIdentityService _serverIdentityService;
        private readonly IPackageMessageValidator _packageMessageValidator;
        private readonly ILogger<PubSubMessageController> logger;

        public PubSubMessageController(IConfiguration configuration, IServiceProvider serviceProvider, IServerIdentityService serverIdentityService, IPackageMessageValidator packageMessageValidator, ILogger<PubSubMessageController> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _serverIdentityService = serverIdentityService;
            _packageMessageValidator = packageMessageValidator;
            this.logger = logger;
        }

        public async void HandleMessage(IPublishedMessage publishedMessage)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Create a scope so all dependency injections are available.
                    var text = Encoding.UTF8.GetString(publishedMessage.DataBytes);

                    var packageMessage = JsonConvert.DeserializeObject<PackageMessage>(text);
                    //if (_serverIdentityService.Id.Equals(packageMessage.ServerId)) // Do not process own package messages
                    //   return;

                    logger.LogInformation($"Received message from {packageMessage.ServerId}");

                    _packageMessageValidator.Validate(packageMessage);

                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var package = mediator.SendAndWait(new FetchPackageCommand(packageMessage.File));

                    if (package == null)
                    {
                        logger.LogError($"Error wrong notitifiation returned from FatchPackageCommand");
                        return;
                    }

                    if (package.Timestamps == null || package.Timestamps.Count == 0)
                    {
                        logger.LogError($"Error no timestamps was found on package id {package.Id} from server id {packageMessage.ServerId}");
                        return;
                    }

                    var packageValidator = scope.ServiceProvider.GetRequiredService<IPackageSchemaValidator>();
                    SchemaValidationResult validationResult = packageValidator.Validate(package); // 
                    if (validationResult.ErrorsFound > 0)
                    {
                        logger.LogError(validationResult.ToString());
                        return;
                    }

                    var timestampValidator = scope.ServiceProvider.GetRequiredService<ITimestampProofValidator>();
                    if (!timestampValidator.Validate(package.Timestamps[0], out IList<string> errors))
                    {
                        var msg = string.Join(", ", errors);
                        logger.LogError(msg);
                        return;
                    }

                    //var peers = await Ipfs.PubSub.PublishAsync;

                    // Now add the package!
                    await mediator.Send(new AddPackageCommand(package));
                }
            }
            catch (Exception ex)
            {
                var msg = $"Message from server {publishedMessage.Sender.Id} failed with: {ex.Message}";
                logger.LogError(msg);
            }

        }
    }
}
