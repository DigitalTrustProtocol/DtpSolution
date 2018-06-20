using System;

namespace DtpGraphCore.Enumerations
{
    [Flags]
    public enum SubjectFlags : byte
    {
        ContainsTrustTrue = 1,
        ClaimsAreArray = 2,
    }
}
