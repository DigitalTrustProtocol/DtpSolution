//using System.Configuration;
//using Microsoft.Extensions.Configuration;
using DtpCore.Builders;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace DtpPackageCore.Configurations
{
    public class PackageConfiguration 
    {
        public static string[] DefaultClaimTypes = new string[] { PackageBuilder.BINARY_TRUST_DTP1, PackageBuilder.ALIAS_IDENTITY_DTP1 };
        public static string[] DefaultClaimScopes = new string[] { "twitter.com" };

        public IList<string> ClaimTypes
        {
            get;
            set;
        }

        public IList<string> ClaimScopes
        {
            get;
            set;
        }

        public PackageConfiguration()
        {

        }

        public static PackageConfiguration GetModel(IConfiguration configuration)
        {
            var model = new PackageConfiguration();
            var packageSection = configuration.GetSection("package");
            model.ClaimTypes = packageSection.GetSection("claimTypes").Get<List<string>>() ?? new List<string>(DefaultClaimTypes);
            model.ClaimScopes = packageSection.GetSection("claimScopes").Get<List<string>>() ?? new List<string>(DefaultClaimScopes);
            //var section = configuration.GetSection("Package") as PackageConfiguration ?? new PackageConfiguration();

            return model;
        }
    }
}
