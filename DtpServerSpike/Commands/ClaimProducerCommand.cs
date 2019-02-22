using DtpCore.Builders;
using DtpCore.Model;
using DtpCore.Service;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnitTest.DtpCore.Extensions;

namespace DtpServerSpike.Commands
{
    [Command(Name = "claimproducer", Description = "Create claims")]
    public class ClaimProducerCommand : CommandBase
    {
        [Argument(0, "claimproducer", "Producing claims")]
        public int ClaimProducer { get; set; }

        Program Parent { get; set; }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            var serverUrl = new Uri("http://localhost/api/package");

            if (ClaimProducer == 0)
                ClaimProducer = 1;

            // Submit claim
            using (var client = new WebClient())
            {
                // Create Claim
                //PackageBuilder 
                Package package = null;
                using (var tt = new TimeMe("Create package"))
                {
                    package = CreatePackage(ClaimProducer);
                }

                using (var tt = new TimeMe("Submit package"))
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    app.Out.WriteLine(client.UploadString(serverUrl, "POST", package.ToString()));
                }
            }

            // Repeat
            return Task.FromResult(0);
        }

        public Package CreatePackage(int count)
        {
            var builder = new PackageBuilder();
            for (int i = 0; i < count; i++)
            {
                var issuer = Guid.NewGuid().ToString();
                var subject = Guid.NewGuid().ToString();
                builder.AddClaimTrue(issuer, subject).CurrentClaim.Scope = "twitter.com";
            }
            builder.Build().Sign();

            return builder.Package; 
        }


    }
}
