using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DtpGraphCore.Enumerations
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QueryFlags : byte
    {
        LeafsOnly = 1,          // Only include result directly to the target[s]
        FullTree = 2,           // Include results from the issuer down to the target[s]
        NONE1 = 4,              // Place holder
        IncludeClaimTrust = 8,  // Include claims that are TrustClaim 
        ServerSign = 16,        // The server signs the result Package with its signature.
        IncludeTimestamp = 32,  // Includes the Trust result timestamp
        NONE2 = 64              // Place holder
    }
}
