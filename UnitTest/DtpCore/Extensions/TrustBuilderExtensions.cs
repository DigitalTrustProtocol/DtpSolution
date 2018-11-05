
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Strategy;

namespace UnitTest.DtpCore.Extensions
{
    public static class TrustBuilderExtensions
    {
        public static IDerivationStrategy ScriptService { get; set; } = new DerivationBTCPKH();

        public static string GetAddress(string name)
        {
            var issuerKey = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(issuerKey);

            return address;
        }

        public static TrustBuilder AddTrust(this TrustBuilder builder, string name)
        {
            var issuerKey = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(issuerKey);

            builder.AddTrust().SetIssuer(address, ScriptService.ScriptName, (byte[] data) =>
            {
                return ScriptService.SignMessage(issuerKey, data);
            });

            return builder;
        }

        public static TrustBuilder AddTrust(this TrustBuilder builder, string issuerName, string subjectName, string type, string attributes)
        {
            builder.AddTrust(issuerName).AddSubject(subjectName, type, attributes);
            return builder;
        }

        public static TrustBuilder AddTrustTrue(this TrustBuilder builder, string issuerName, string subjectName)
        {
            builder.AddTrust(issuerName, subjectName, TrustBuilder.BINARY_TRUST_DTP1,  TrustBuilder.CreateBinaryTrustAttributes());
            return builder;
        }

        public static TrustBuilder AddSubject(this TrustBuilder builder, string subjectName, string type, string attributes)
        {
            var key = ScriptService.GetKey(Encoding.UTF8.GetBytes(subjectName));
            var address = ScriptService.GetAddress(key);

            builder.AddSubject(address);
            builder.AddType(type, attributes);
            return builder;
        }

        public static TrustBuilder SetServer(this TrustBuilder builder, string name)
        {
            var key = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(key);

            builder.SetServer(address, ScriptService.ScriptName, (byte[] data) =>
            {
                return ScriptService.SignMessage(key, data);
            });

            return builder;
        }

        public static Trust BuildBinaryTrust(this TrustBuilder builder, string issuerName, string subjectName, bool claim, uint created = 0)
        {
            builder.SetServer("testserver");
            builder.AddTrust(issuerName, subjectName, TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(claim));

            if (created > 0)
                builder.CurrentTrust.Created = created;

            builder.Build().Sign();
            return builder.CurrentTrust;
        }

        //public static TrustBuilder AddClaim(this TrustBuilder builder, JObject data, out Claim claim)
        //{
        //    claim = new Claim
        //    {
        //        Data = data.ToString(Formatting.None)
        //    };

        //    builder.AddClaim(claim);

        //    return builder;
        //}

    }
}
