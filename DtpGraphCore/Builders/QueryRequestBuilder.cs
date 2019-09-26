using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Enumerations;
using DtpGraphCore.Model;

namespace DtpGraphCore.Builders
{
    public class QueryRequestBuilder
    {
        public QueryRequest Query { get; }

        public QueryRequestBuilder(string type) : this("", type)
        {
        }

        public QueryRequestBuilder(string scope, string type)
        {
            Query = new QueryRequest
            {
                Issuer = new Identity(),
                Subjects = new List<string>(),
                Scope = scope,
                Types = new List<string>() { type },
                Flags = QueryFlags.FullTree
            };
        }

        public QueryRequestBuilder Add(string issuerId, string subjectAddress)
        {
            Query.Issuer.Type = "thing";
            Query.Issuer.Id = issuerId;
            Query.Subjects.Add(subjectAddress);

            return this;
        }
    }
}
