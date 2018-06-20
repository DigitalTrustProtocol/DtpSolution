using System.Collections.Generic;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;

namespace DtpGraphCore.Builders
{
    public class QueryRequestBuilder
    {
        public QueryRequest Query { get; }

        public QueryRequestBuilder(string type) : this(null, type)
        {
        }

        public QueryRequestBuilder(TrustScope scope, string type)
        {
            Query = new QueryRequest
            {
                Issuer = null,
                Subjects = new List<SubjectQuery>(),
                Scope = scope,
                Types = new List<string>() { type },
                Flags = QueryFlags.FullTree
            };
        }

        public QueryRequestBuilder Add(byte[] issuerId, byte[] subjectAddress)
        {
            Query.Issuer = issuerId;
            Query.Subjects.Add(new SubjectQuery { Address = subjectAddress });

            return this;
        }
    }
}
