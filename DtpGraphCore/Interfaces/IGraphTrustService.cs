using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Model;

namespace DtpGraphCore.Interfaces
{
    public interface IGraphTrustService
    {
        GraphModel Graph { get; set; }

        int GlobalScopeIndex { get; set; }
        int BinaryTrustTypeIndex { get; set; }

        void Add(Package package);
        void Add(IEnumerable<Trust> trusts);
        void Add(Trust trust);
        void Remove(Trust trust);

        GraphSubject CreateGraphSubject(string subjectAddress);
        //void InitSubjectModel(Subject node, Claim claim, GraphSubject edge);
        GraphIssuer EnsureGraphIssuer(string address);
        //GraphClaim EnsureSubjectClaim(GraphSubject graphSubject, Claim trustClaim);
        GraphClaim EnsureGraphClaim(Trust trust);
        GraphClaim CreateGraphClaim(Trust trust);
        GraphClaim CreateGraphClaim(string type, string scope, string attributes);
        int GetClaimDataIndex(Trust trust);
        GraphSubject EnsureGraphSubject(GraphIssuer graphIssuer, string subjectAddress);
        void BuildPackage(QueryContext context);

    }
}