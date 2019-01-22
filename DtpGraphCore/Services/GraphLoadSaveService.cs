using System.Linq;
using Microsoft.Extensions.Logging;
using DtpCore.Interfaces;
using DtpGraphCore.Interfaces;
using System;
using DtpCore.Extensions;

namespace DtpGraphCore.Services
{
    public class GraphLoadSaveService : IGraphLoadSaveService
    {
        private IGraphClaimService _graphTrustService;

        private ITrustDBService _trustDBService;
        private ILogger _logger;

        public GraphLoadSaveService(IGraphClaimService graphTrustService, ITrustDBService trustDBService, ILoggerFactory loggerFactory)
        {
            _graphTrustService = graphTrustService;
            _trustDBService = trustDBService;
            _logger = loggerFactory.CreateLogger<GraphLoadSaveService>();
        }

        public void LoadFromDatabase()
        {
            _logger.LogInformation("Loading trust into Graph");
            var count = 0;

            // No need to load packages, just load trusts directly.
            var trusts = _trustDBService.GetActiveClaims();

            foreach (var trust in trusts)
            {
                count++;
                _graphTrustService.Add(trust);
            }
            _logger.LogInformation($"Trust loaded: {count}");
        }


        //public void LoadGraphSnapshot(string filename)
        //{
        //    _graphTrustService.Graph = JsonConvert.DeserializeObject<GraphModel>(File.ReadAllText(filename));
        //}

        //public void SaveGraphSnapshot(string filename)
        //{
        //    File.WriteAllText(filename, JsonConvert.SerializeObject(_graphTrustService.Graph, Formatting.Indented));
        //}

    }
}
