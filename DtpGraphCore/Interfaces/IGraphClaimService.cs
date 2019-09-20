using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Model;

namespace DtpGraphCore.Interfaces
{
    public interface IGraphClaimService
    {
        GraphModel Graph { get; set; }

        //int GlobalScopeIndex { get; set; }
        //int BinaryClaimTypeIndex { get; set; }


        void Add(Package package);
        void Add(IEnumerable<Claim> claims);
        void Add(Claim claim);
        void Remove(Claim claim);

        /// <summary>
        /// Removes all claims in the graph belonging to the provided issuer.
        /// </summary>
        /// <param name="claim"></param>
        void RemoveByIssuer(Claim claim);

        GraphSubject CreateGraphSubject(string subjectId);
        GraphIssuer EnsureGraphIssuer(string address);
        GraphClaim EnsureGraphClaim(Claim claim);
        GraphClaim CreateGraphClaim(Claim claim);
        GraphClaim CreateGraphClaim(string type, string scope, string attributes);
        int GetClaimDataIndex(Claim claim);
        GraphSubject EnsureGraphSubject(GraphIssuer graphIssuer, string subjectId);
        void BuildPackage(QueryContext context);

    }
}