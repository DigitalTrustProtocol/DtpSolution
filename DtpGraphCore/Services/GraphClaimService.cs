﻿using System.Collections.Generic;
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
    public class GraphClaimService : IGraphClaimService
    {
        public GraphModel Graph { get; set;}
        public IPackageSchemaService TrustSchema { get; }

        public int GlobalScopeIndex { get; set; }
        public int BinaryClaimTypeIndex { get; set; }

        public GraphClaimService(GraphModel graph, IPackageSchemaService trustSchemaService)
        {
            Graph = graph;
            TrustSchema = trustSchemaService;
        }

        public void Add(Package package)
        {
            Add(package.Claims);
        }

        public void Add(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                Add(claim);
            }
        }

        public void Add(Claim claim)
        {
            var issuer = EnsureGraphIssuer(claim.Issuer.Id);

            var graphSubject = EnsureGraphSubject(issuer, claim.Subject.Id);

            var graphClaim = EnsureGraphClaim(claim);
            graphSubject.Claims.Ensure(graphClaim.Scope, graphClaim.Type, graphClaim.Index);
        }

        public void Remove(Claim claim)
        {
            if (!Graph.IssuerIndex.TryGetValue(claim.Issuer.Id, out int issuerIndex))
                return; // No issuer, then no trust!

            if (!Graph.IssuerIndex.TryGetValue(claim.Subject.Id, out int subjectIndex))
                return; // No subject, then no trust!

            var graphIssuer = Graph.Issuers[issuerIndex];
            if (!graphIssuer.Subjects.ContainsKey(subjectIndex))
                return; // No subject to the issuer to be removed!

            var subject = graphIssuer.Subjects[subjectIndex];


            if (!Graph.ClaimType.TryGetKey(claim.Type, out int claimTypeIndex))
                return; // Scope was not found !

            int scopeIndex = -1;
            if (!Graph.Scopes.TryGetKey(claim.Scope, out scopeIndex))
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

        public GraphClaim EnsureGraphClaim(Claim trust)
        {
            var graphClaim = CreateGraphClaim(trust);

            var id = graphClaim.ID();
            if (!Graph.ClaimIndex.TryGetValue(id, out int index))
            {
                graphClaim.Index = Graph.Claims.Count;

                if (PackageBuilder.IsTrustTrue(trust.Type, trust.Value))
                    graphClaim.Flags |= ClaimFlags.Trust;

                Graph.Claims.Add(graphClaim);
                Graph.ClaimIndex.Add(id, graphClaim.Index);

                return graphClaim;
            }

            return Graph.Claims[index];

        }
        public GraphClaim CreateGraphClaim(Claim trust)
        {
            var trustTypeString = TrustSchema.GetTrustTypeString(trust);
            return CreateGraphClaim(trustTypeString, trust.Scope, trust.Value);
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

        public int GetClaimDataIndex(Claim trust)
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
                Claims = new List<Claim>(context.TrackerResults.Count)
            };

            foreach (var tracker in context.TrackerResults.Values)
            {
                foreach (var ts in tracker.Subjects.Values)
                {
                    if(ts.Claims.Count() == 0)
                    {
                        var claim = new Claim
                        {
                            Issuer = new IssuerIdentity { Id = tracker.Issuer.Id },
                            Subject = new SubjectIdentity { Id = ts.TargetIssuer.Id }
                        };

                        claim.Type = PackageBuilder.BINARY_TRUST_DTP1;
                        claim.Value = PackageBuilder.CreateBinaryTrustAttributes(true);

                        context.Results.Claims.Add(claim);
                    }
                    else
                    {
                        foreach (var claimEntry in ts.Claims)
                        {
                            var claim = new Claim
                            {
                                Issuer = new IssuerIdentity { Id = tracker.Issuer.Id },
                                Subject = new SubjectIdentity { Id = ts.TargetIssuer.Id }
                            };

                            var claimIndex = claimEntry.Value;
                            var trackerClaim = Graph.Claims[claimIndex];

                            if (Graph.ClaimType.TryGetValue(trackerClaim.Type, out string type))
                                claim.Type = type;

                            if (Graph.ClaimAttributes.TryGetValue(trackerClaim.Attributes, out string attributes))
                                claim.Value = attributes;

                            if (Graph.Scopes.TryGetValue(trackerClaim.Scope, out string scope))
                                claim.Scope = scope;

                            claim.Expire = 0;
                            claim.Activate = 0;

                            context.Results.Claims.Add(claim);
                        }
                    }
                }
            }
        }
    }
}
