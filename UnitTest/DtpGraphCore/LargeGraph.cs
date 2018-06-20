using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DtpCore.Builders;
using DtpCore.Model;
using DtpGraphCore.Builders;
using UnitTest.DtpCore.Extensions;

namespace UnitTest.DtpGraphCore
{
    [TestClass]
    public class LargeGraph : TrustGraphMock
    {
        //[TestMethod]
        //public void Test1()
        //{
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    var target = "";
        //    int factor = 20;

        //    for (int x = 1; x <= factor; x++)
        //    {
        //        var Level1 = $"L1_{x}";
        //        _trustBuilder.AddTrust($"L0", Level1, BinaryTrustTrueAttributes);
        //        for (int y = 1; y <= factor; y++)
        //        {
        //            var xfactor = x * factor;
        //            var Level2 = $"L2_{(xfactor) + y}";
        //            _trustBuilder.AddTrust(Level1, Level2, BinaryTrustTrueAttributes);
        //            for (int z = 1; z <= factor; z++)
        //            {
        //                var yfactor = (xfactor + y) * factor;
        //                var Level3 = $"L3_{yfactor + z}";
        //                target = Level3;
        //                _trustBuilder.AddTrust(Level2, Level3, BinaryTrustTrueAttributes);
        //            }
        //        }

        //        //_trustBuilder.AddTrust($"{y}", $"{x}", ClaimTrustTrueTest);
        //    }


        //    _graphTrustService.Add(_trustBuilder.Package);
        //    //Console.WriteLine(JsonConvert.SerializeObject(_graphTrustService.Graph, Formatting.Indented));
        //    watch.Stop();
        //    Console.WriteLine($"Build: {watch.ElapsedMilliseconds}");

        //    watch.Restart();
        //    var queryBuilder = new QueryRequestBuilder(BinaryTrustTrueAttributes.Type);
        //    BuildQuery(queryBuilder, $"L0", target);

        //    for (int i = 0; i < 100; i++)
        //    {


        //        // Execute
        //        var context = _graphQueryService.Execute(queryBuilder.Query);

        //        if(i % 99 == 0)
        //            Console.WriteLine($"Results: ${context.Results.Trusts.Count}");
        //    }

        //    watch.Stop();
        //    Console.WriteLine($"Search: {watch.ElapsedMilliseconds}");

        //}

    }
}
