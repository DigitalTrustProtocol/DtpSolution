using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DtpCore.Collections.Generic;

namespace DtpGraphCore.Model
{
    public class GraphModel
    {
        //public ConcurrentDictionary<string, int> IssuerIndex = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        public Dictionary<string, int> IssuerIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        public List<GraphIssuer> Issuers = new List<GraphIssuer>();
        public List<GraphClaim> Claims = new List<GraphClaim>();
        //public ConcurrentList<GraphIssuer> Issuers = new ConcurrentList<GraphIssuer>();
        //public ConcurrentList<GraphClaim> Claims = new ConcurrentList<GraphClaim>();
        //public ConcurrentDictionary<string, int> ClaimIndex = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        public Dictionary<string, int> ClaimIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        public DictionaryTwoWay<string> ClaimType = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        public DictionaryTwoWay<string> ClaimAttributes = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        public DictionaryTwoWay<string> Scopes = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        //public DictionaryTwoWay<string> Alias = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
        //public DictionaryTwoWay<string> Notes = new DictionaryTwoWay<string>(StringComparer.OrdinalIgnoreCase);
    }
}
