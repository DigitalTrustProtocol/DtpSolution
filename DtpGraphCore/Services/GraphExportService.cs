using System.Collections.Generic;
using DtpCore.Model;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Services
{
    public class GraphExportService : IGraphExportService
    {
        public IGraphTrustService ModelService { get; }

        public GraphExportService(IGraphTrustService modelService)
        {
            ModelService = modelService;
        }

        public Package GetFullGraph()
        {
            return null;
            //var package = new Package();
            //package.Trusts = new List<Trust>();

            //foreach (var address in ModelService.Graph.Issuer)
            //{
            //    var trust = new Trust();
            //    trust.Issuer = new Identity();
            //    trust.Issuer.Address = address.Id;
            //    trust.Claims = new List<Claim>();
            //    var subjects = new List<Subject>();

            //    if (address.Subjects != null)
            //    {
            //        foreach (var edge in address.Subjects)
            //        {
            //            var child = new Subject();
            //            var claim = new Claim();
            //            ModelService.InitSubjectModel(child, claim, edge);

            //            subjects.Add(child);
            //            trust.Claims.Add(claim);
            //        }
            //    }
            //    if(subjects.Count > 0)
            //        trust.Subjects = subjects;

            //    package.Trusts.Add(trust);
            //}
            //return package;
        }

    }
}
