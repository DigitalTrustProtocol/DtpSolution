﻿using System;
using System.Collections.Generic;
using DtpCore.Extensions;
using Newtonsoft.Json;
using DtpGraphCore.Interfaces;
using System.Runtime.InteropServices;
using DtpCore.Collections.Generic;
using System.Security.Claims;
using DtpCore.Builders;
using DtpCore.Collections;
using DtpGraphCore.Enumerations;
using DtpCore.Model;

namespace DtpGraphCore.Model
{

    [JsonObject(MemberSerialization.OptIn)]
    public class QueryContext
    {
        [JsonIgnore]
        public IGraphClaimService GraphTrustService = null;

        [JsonIgnore]
        public GraphIssuer Issuer = null;

        [JsonIgnore]
        public Dictionary<int, GraphIssuer> Targets = null;

        [JsonIgnore]
        public List<int> ClaimTypes;

        [JsonIgnore]
        public int ClaimScope = 0;

        [JsonIgnore]
        public Stack<GraphTracker> Tracker = new Stack<GraphTracker>();

        [JsonIgnore]
        public Dictionary<int, GraphTracker> TrackerResults = new Dictionary<int, GraphTracker>();

        [JsonIgnore]
        public int GlobalScopeIndex { get; set; }
        [JsonIgnore]
        public int BinaryClaimTypeIndex { get; set; }




        [JsonIgnore]
        public int MaxCost = 0;

        /// <summary>
        /// Use field and not get and set methods. This may be quicker.
        /// </summary>
        [JsonIgnore]
        public int Level = 0;

        [JsonIgnore]
        public int MaxLevel = 0;

        [JsonIgnore]
        public BitArrayFast Visited;

        [JsonIgnore]
        public QueryFlags Flags;


        [JsonIgnore]
        internal Dictionary<int, GraphIssuer> TargetsFound = new Dictionary<int, GraphIssuer>();

        public bool ShallowResult = false;

        [JsonProperty(PropertyName = "results", NullValueHandling = NullValueHandling.Ignore, Order = 50)]
        public Package Results { get; set; }

        [JsonProperty(PropertyName = "IssuerCount")]
        public int IssuerCount = 0;

        [JsonProperty(PropertyName = "SubjectCount")]
        public int SubjectCount = 0;

        [JsonProperty(PropertyName = "errors", NullValueHandling = NullValueHandling.Ignore, Order = 80)]
        public List<string> Errors { get; set; }
        public bool ShouldSerializeErrors()
        {
            return Errors != null && Errors.Count > 0;
        }

        public QueryContext()
        {
            Issuer = new GraphIssuer();
            Targets = new Dictionary<int, GraphIssuer>(); 
            ClaimTypes = new List<int>();
            Errors = new List<string>();

            MaxCost = 500; 
            Level = 0;
            MaxLevel = 3; // About 3 levels down max!
        }

        public QueryContext(IGraphClaimService graphService, QueryRequest query) : this()
        {
            GraphTrustService = graphService;

            SetupIssuers(query);
            SetupSubjects(query);
            SetupQueryClaim(query);
            SetupIndexs();

            if (query.Level > 0 && query.Level < MaxLevel)
                MaxLevel = query.Level;

            Flags = query.Flags;

            Visited = new BitArrayFast(GraphTrustService.Graph.Issuers.Count + 1024, false); // 1024 is buffer for new Issuers when searching
        }

        private void SetupIndexs()
        {
            if (GraphTrustService.Graph.ClaimType.TryGetKey(PackageBuilder.BINARY_CLAIM_DTP1, out int claimIndex))
                BinaryClaimTypeIndex = claimIndex;

            if (GraphTrustService.Graph.Scopes.TryGetKey("", out int scopeIndex))
                GlobalScopeIndex = scopeIndex;
        }

        internal void SetupIssuers(QueryRequest query)
        {

            if (!GraphTrustService.Graph.IssuerIndex.ContainsKey(query.Issuer.Id))
            {
                Errors.Add($"Unknown Issuer {query.Issuer.Id}");
                return;
            }

            var index = GraphTrustService.Graph.IssuerIndex[query.Issuer.Id];
            Issuer =  GraphTrustService.Graph.Issuers[index];
        }

        internal void SetupSubjects(QueryRequest query)
        {
            foreach (var subject in query.Subjects)
            {
                if (GraphTrustService.Graph.IssuerIndex.ContainsKey(subject))
                {
                    var index = GraphTrustService.Graph.IssuerIndex[subject];
                    Targets[index] = GraphTrustService.Graph.Issuers[index];
                }
                else
                    Errors.Add($"Unknown subject {subject}");
            }

        }

        internal void SetupQueryClaim(QueryRequest query)
        {
            if (!GraphTrustService.Graph.Scopes.ContainsKey(query.Scope))
                Errors.Add($"Unknown claim scope {query.Scope}");
            else
                ClaimScope = GraphTrustService.Graph.Scopes.GetIndex(query.Scope);

            // If no types have been defined then use the defaults
            if (query.Types == null || query.Types.Count == 0)
            {
                var graphClaim = GraphTrustService.CreateGraphClaim(PackageBuilder.BINARY_CLAIM_DTP1, "", PackageBuilder.CreateBinaryTrustAttributes(true));
                ClaimTypes.Add(graphClaim.Index);
                //var idClaim = GraphTrustService.CreateGraphClaim(PackageBuilder.ID_IDENTITY_DTP1, "", PackageBuilder.CreateBinaryTrustAttributes(true));
                //ClaimTypes.Add(idClaim.Index);

            }
            else
            {
                foreach (var type in query.Types)
                {
                    if (!GraphTrustService.Graph.ClaimType.ContainsKey(type))
                        Errors.Add($"Unknown claim type {type}"); // Why!?
                    else
                        ClaimTypes.Add(GraphTrustService.Graph.ClaimType.GetIndex(type));
                }

                if (!ClaimTypes.Contains(this.BinaryClaimTypeIndex))
                    Flags |= QueryFlags.IncludeClaimTrust;
            }
        }
    }
}
