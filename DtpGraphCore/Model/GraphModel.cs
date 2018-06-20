using System;
using System.Collections.Generic;
using DtpCore.Collections.Generic;

namespace DtpGraphCore.Model
{
    public class GraphModel
    {
        public Dictionary<byte[], int> IssuerIndex = new Dictionary<byte[], int>(ByteComparer.Standard);
        public List<GraphIssuer> Issuers = new List<GraphIssuer>();

        public List<GraphClaim> Claims = new List<GraphClaim>();
        public Dictionary<string, int> ClaimIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        public DictionaryTwoWay<string> ClaimType = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        public DictionaryTwoWay<string> ClaimAttributes = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        public DictionaryTwoWay<string> Scopes = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        public DictionaryTwoWay<string> Alias = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
    }
}
