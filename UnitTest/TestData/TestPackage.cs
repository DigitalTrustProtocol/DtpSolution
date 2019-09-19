using DtpCore.Builders;
using DtpCore.Model;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.TestData
{
    public class TestPackage
    {

        public static Package CreateBinary(int claims = 1)
        {
            var builder = CreateUnsigned(claims, PackageBuilder.BINARY_CLAIM_DTP1, PackageBuilder.CreateBinaryTrustAttributes(true));

            builder.Build();
            builder.Sign();

            return builder.Package;
        }

        public static Package Create(int claims, string type, string value)
        {
            var builder = CreateUnsigned(claims, type, value);

            builder.Build();
            builder.Sign();

            return builder.Package;
        }

        public static PackageBuilder CreateUnsigned(int claims = 1, string type = PackageBuilder.BINARY_CLAIM_DTP1, string value = "true")
        {
            var builder = new PackageBuilder();
            builder.SetServer("testserver");
            for (int i = 0; i < claims; i++)
            {
                builder.AddClaim($"issuerName{i}", $"subjectName{i}", type, value).BuildClaimID();
            }

            return builder;
        }


        private Claim CreateBinaryClaim(string issuerName, string subjectName, bool value = true)
        {
            var builder = new PackageBuilder();
            builder.AddClaim(issuerName, subjectName, PackageBuilder.BINARY_CLAIM_DTP1, PackageBuilder.CreateBinaryTrustAttributes(value)).BuildClaimID();

            return builder.CurrentClaim;
        }

    }
}
