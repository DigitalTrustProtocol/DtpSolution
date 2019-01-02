using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Model;

namespace DtpGraphCore.Interfaces
{
    public interface IGraphClaimService
    {
        GraphModel Graph { get; set; }

        int GlobalScopeIndex { get; set; }
        int BinaryClaimTypeIndex { get; set; }

        void Add(Package package);
        void Add(IEnumerable<Claim> trusts);
        void Add(Claim trust);
        void Remove(Claim trust);

        GraphSubject CreateGraphSubject(string subjectId);
        GraphIssuer EnsureGraphIssuer(string address);
        GraphClaim EnsureGraphClaim(Claim trust);
        GraphClaim CreateGraphClaim(Claim trust);
        GraphClaim CreateGraphClaim(string type, string scope, string attributes);
        int GetClaimDataIndex(Claim trust);
        GraphSubject EnsureGraphSubject(GraphIssuer graphIssuer, string subjectId);
        void BuildPackage(QueryContext context);

    }
}