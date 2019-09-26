using System.Text;
using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using DtpCore.Strategy;

namespace UnitTest.DtpCore.Extensions
{
    public static class PackageBuilderExtensions
    {
        public static IDerivationStrategy ScriptService { get; set; } = new DerivationSecp256k1PKH();


        public static string GetAddress(string name)
        {
            var issuerKey = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(issuerKey);

            return address;
        }

        public static PackageBuilder AddClaim(this PackageBuilder builder, string name)
        {
            var issuerKey = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(issuerKey);

            builder.AddClaim().SetIssuer(address, "entity", (byte[] data) =>
            {
                return ScriptService.SignMessage(issuerKey, data);
            });

            return builder;
        }

        public static PackageBuilder AddClaim(this PackageBuilder builder, string issuerName, string subjectName, string type, string attributes)
        {
            builder.AddClaim(issuerName).AddSubject(subjectName, type, attributes);
            return builder;
        }

        public static PackageBuilder AddClaimTrue(this PackageBuilder builder, string issuerName, string subjectName)
        {
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_CLAIM_DTP1,  PackageBuilder.CreateBinaryTrustAttributes());
            return builder;
        }

        public static PackageBuilder AddRating(this PackageBuilder builder, string issuerName, string subjectName, int value)
        {
            builder.AddClaim(issuerName, subjectName, PackageBuilder.RATING_CLAIM_DTP1, value.ToString());
            return builder;
        }


        public static PackageBuilder AddSubject(this PackageBuilder builder, string subjectName, string type, string attributes)
        {
            var key = ScriptService.GetKey(Encoding.UTF8.GetBytes(subjectName));
            var address = ScriptService.GetAddress(key);

            builder.AddSubject(address);
            builder.AddType(type, attributes);
            return builder;
        }

        public static PackageBuilder SetServer(this PackageBuilder builder, string name)
        {
            var key = ScriptService.GetKey(Encoding.UTF8.GetBytes(name));
            var address = ScriptService.GetAddress(key);

            builder.SetServer(address, ScriptService.ScriptName, (byte[] data) =>
            {
                return ScriptService.SignMessage(key, data);
            });

            return builder;
        }

        public static Claim BuildBinaryClaim(this PackageBuilder builder, string issuerName, string subjectName, bool claim, uint created = 0)
        {
            builder.SetServer("testserver");
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_CLAIM_DTP1, PackageBuilder.CreateBinaryTrustAttributes(claim));

            if (created > 0)
                builder.CurrentClaim.Created = created;

            builder.Build().Sign();
            return builder.CurrentClaim;
        }

        public static Claim BuildClaim(this PackageBuilder builder, string type, string issuerName, string subjectName, bool claim, uint created = 0)
        {
            builder.SetServer("testserver");
            builder.AddClaim(issuerName, subjectName, type, PackageBuilder.CreateBinaryTrustAttributes(claim));

            if (created > 0)
                builder.CurrentClaim.Created = created;

            builder.Build().Sign();
            return builder.CurrentClaim;
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
