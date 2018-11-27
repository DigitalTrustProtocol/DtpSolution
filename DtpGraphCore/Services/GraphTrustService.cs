using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Model;
using DtpGraphCore.Interfaces;
using DtpCore.Builders;
using DtpGraphCore.Extensions;
using DtpGraphCore.Enumerations;
using System.Linq;
using DtpCore.Extensions;
using DtpCore.Interfaces;
using System.Collections.Concurrent;

namespace DtpGraphCore.Services
{
    public class GraphTrustService : IGraphTrustService
    {
        public GraphModel Graph { get; set;}
        public ITrustSchemaService TrustSchema { get; }

        public int GlobalScopeIndex { get; set; }
        public int BinaryTrustTypeIndex { get; set; }

        public GraphTrustService(GraphModel graph, ITrustSchemaService trustSchemaService)
        {
            Graph = graph;
            TrustSchema = trustSchemaService;
        }

        public void Add(Package package)
        {
            Add(package.Trusts);
        }

        public void Add(IEnumerable<Trust> trusts)
        {
            foreach (var trust in trusts)
            {
                Add(trust);
            }
        }

        public void Add(Trust trust)
        {
            var issuer = EnsureGraphIssuer(trust.Issuer.Id);

            var graphSubject = EnsureGraphSubject(issuer, trust.Subject.Id);

            var graphClaim = EnsureGraphClaim(trust);
            graphSubject.Claims.Ensure(graphClaim.Scope, graphClaim.Type, graphClaim.Index);
        }

        public void Remove(Trust trust)
        {
            if (!Graph.IssuerIndex.TryGetValue(trust.Issuer.Id, out int issuerIndex))
                return; // No issuer, then no trust!

            if (!Graph.IssuerIndex.TryGetValue(trust.Subject.Id, out int subjectIndex))
                return; // No subject, then no trust!

            var graphIssuer = Graph.Issuers[issuerIndex];
            if (!graphIssuer.Subjects.ContainsKey(subjectIndex))
                return; // No subject to the issuer to be removed!

            var subject = graphIssuer.Subjects[subjectIndex];


            if (!Graph.ClaimType.TryGetKey(trust.Type, out int claimTypeIndex))
                return; // Scope was not found !

            int scopeIndex = -1;
            if (!Graph.Scopes.TryGetKey(trust.Scope, out scopeIndex))
                return; // Scope was not found !

            //var graphClaim = CreateGraphClaim(trust);
            //var id = graphClaim.ID();

            //if (!Graph.ClaimIndex.TryGetValue(id, out int claimIndex))
            //    return; // No cliam, no trust to remove!

            //var claim = Graph.Claims[claimIndex];
            //if (!subject.Claims.GetIndex(claim.Scope, claim.Type, out int subjectClaimIndex))
            //    return; // No claim on subject that is a match;

            var claimIndex = subject.Claims.Remove(scopeIndex, claimTypeIndex);
            if (claimIndex < 0)
                return; // There was no claim found
            
            // Its currently no prossible to remove GraphClaim object, as we do not know if any other is referencing to it.


            if (subject.Claims.Count > 0)
                return; // There are more claims, therefore do not remove subject.

            graphIssuer.Subjects.Remove(subjectIndex);
            if (graphIssuer.Subjects.Count > 0)
                return; // There are more subjects, therefore do not remove issuer.

            // Is it possble to remove the issuer?, as we do not know if any other is referencing to it.
            // There is no backpointer, so this would be a DB query.
        }

        public GraphIssuer EnsureGraphIssuer(string issuerId)
        {

            if (!Graph.IssuerIndex.TryGetValue(issuerId, out int index))
            {
                index = Graph.Issuers.Count;
                var issuer = new GraphIssuer { Id = issuerId, Index = index };
                Graph.Issuers.Add(issuer);
                Graph.IssuerIndex.Add(issuerId, index);
                return issuer;
            }

            return Graph.Issuers[index];
        }

        public GraphSubject EnsureGraphSubject(GraphIssuer graphIssuer, string subjectId)
        {
            var index = EnsureGraphIssuer(subjectId).Index;
            if (!graphIssuer.Subjects.ContainsKey(index))
            {
                var graphSubject = CreateGraphSubject(subjectId);
                graphIssuer.Subjects.Add(index, graphSubject);
            }
            return graphIssuer.Subjects[index];
        }

        public GraphSubject CreateGraphSubject(string subjectId)
        {
            var graphSubject = new GraphSubject
            {
                TargetIssuer =  EnsureGraphIssuer(subjectId),
                Claims = new ConcurrentDictionary<long, int>()
            };

            return graphSubject;
        }

        public GraphClaim EnsureGraphClaim(Trust trust)
        {
            var graphClaim = CreateGraphClaim(trust);

            var id = graphClaim.ID();
            if (!Graph.ClaimIndex.TryGetValue(id, out int index))
            {
                graphClaim.Index = Graph.Claims.Count;

                if (TrustBuilder.IsTrustTrue(trust.Type, trust.Claim))
                    graphClaim.Flags |= ClaimFlags.Trust;

                Graph.Claims.Add(graphClaim);
                Graph.ClaimIndex.Add(id, graphClaim.Index);

                return graphClaim;
            }

            return Graph.Claims[index];

        }
        public GraphClaim CreateGraphClaim(Trust trust)
        {
            var trustTypeString = TrustSchema.GetTrustTypeString(trust);
            return CreateGraphClaim(trustTypeString, trust.Scope, trust.Claim);
        }

        public GraphClaim CreateGraphClaim(string type, string scope, string attributes)
        {
            var gclaim = new GraphClaim
            {
                Type = Graph.ClaimType.Ensure(type),
                Scope = Graph.Scopes.Ensure(scope),
                Attributes = Graph.ClaimAttributes.Ensure(attributes),
                Flags = 0
            };
            return gclaim;
        }

        public int GetClaimDataIndex(Trust trust)
        {
            var graphClaim = CreateGraphClaim(trust);
            var index = Graph.ClaimIndex.GetValueOrDefault(graphClaim.ID());
            return index;
        }

        /// <summary>
        /// Build a result package from the TrackerResults
        /// </summary>
        /// <param name="context"></param>
        public void BuildPackage(QueryContext context)
        {
            // Clear up the result

            context.Results = new Package
            {
                Trusts = new List<Trust>(context.TrackerResults.Count)
            };

            foreach (var tracker in context.TrackerResults.Values)
            {
                foreach (var ts in tracker.Subjects.Values)
                {
                    if(ts.Claims.Count() == 0)
                    {
                        var trust = new Trust
                        {
                            Issuer = new IssuerIdentity { Id = tracker.Issuer.Id },
                            Subject = new SubjectIdentity { Id = ts.TargetIssuer.Id }
                        };

                        trust.Type = TrustBuilder.BINARY_TRUST_DTP1;
                        trust.Claim = TrustBuilder.CreateBinaryTrustAttributes(true);

                        context.Results.Trusts.Add(trust);
                    }
                    else
                    {
                        foreach (var claimEntry in ts.Claims)
                        {
                            var trust = new Trust
                            {
                                Issuer = new IssuerIdentity { Id = tracker.Issuer.Id },
                                Subject = new SubjectIdentity { Id = ts.TargetIssuer.Id }
                            };

                            var claimIndex = claimEntry.Value;
                            var trackerClaim = Graph.Claims[claimIndex];

                            if (Graph.ClaimType.TryGetValue(trackerClaim.Type, out string type))
                                trust.Type = type;

                            if (Graph.ClaimAttributes.TryGetValue(trackerClaim.Attributes, out string attributes))
                                trust.Claim = attributes;

                            if (Graph.Scopes.TryGetValue(trackerClaim.Scope, out string scope))
                                trust.Scope = scope;

                            trust.Expire = 0;
                            trust.Activate = 0;

                            context.Results.Trusts.Add(trust);
                        }
                    }
                }
            }
        }
    }
}
