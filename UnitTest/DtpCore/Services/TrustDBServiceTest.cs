using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using DtpCore.Builders;
using DtpCore.Repository;
using DtpCore.Services;
using DtpCore.Strategy;
using DtpCore.Extensions;
using DtpCore.Factories;
using UnitTest.DtpCore.Extensions;
using DtpCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using DtpCore.Model;
using System.Collections;

namespace UnitTest.DtpCore.Builders
{
    [TestClass]
    public class TrustDBServiceTest : StartupMock
    {
        [TestMethod]
        public void AddPackage()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("testissuer1")
                .AddSubject("testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();

            trustDBService.Add(builder.Package);
            //trustDBService.DBContext.SaveChanges();

            var dbPackage = trustDBService.Packages.FirstOrDefaultAsync().Result;

            var compareResult = builder.Package.JsonCompare(dbPackage);
            Assert.IsTrue(compareResult, "Package from database is not the same as Builder");
        }

        [TestMethod]
        public void Add1Trust()
        {

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("testissuer1", "testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();

            Assert.IsNull(builder.CurrentTrust.PackageDatabaseID);

            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            trustDBService.Add(builder.CurrentTrust);

            trustDBService.DBContext.SaveChanges();

            var dbTrust = trustDBService.Trusts.FirstOrDefaultAsync().Result;
            var compareResult = builder.CurrentTrust.JsonCompare(dbTrust);
            Assert.IsTrue(compareResult, "Trust from database is not the same as Builder");
        }

        [TestMethod]
        public void GetPackageLessTrust()
        {

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrustTrue("testissuer1", "testsubject1")
                .AddTrustTrue("testissuer2", "testsubject2")
                .Build()
                .Sign();

            Assert.IsNull(builder.CurrentTrust.PackageDatabaseID);

            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            foreach (var trust in builder.Package.Trusts)
                trustDBService.Add(trust);

            var builder2 = new TrustBuilder(ServiceProvider);
            builder2.SetServer("testserver");
            builder2.AddTrustTrue("A", "D")
                .AddTrustTrue("B", "C")
                .Build()
                .Sign();

            trustDBService.Add(builder2.Package);
            trustDBService.DBContext.SaveChanges();

            var dbTrusts = trustDBService.Trusts.Where(p=>p.PackageDatabaseID == null);

            Assert.AreEqual(2, dbTrusts.Count(), "There should be 2 trusts not in a package.");
            foreach (var dbTrust in dbTrusts)
            {
                var compareResult = builder.Package.Trusts.First(p=>p.Id == dbTrust.Id).JsonCompare(dbTrust);
                Assert.IsTrue(compareResult, "Trust from database is not the same as Builder");
            }
        }


        [TestMethod]
        public void Add2Claims()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();

            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("testissuer1")
                .AddSubject("testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .AddTrust("testissuer2")
                .AddSubject("testsubject2", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();

            Console.WriteLine(JsonConvert.SerializeObject(builder.Package, Formatting.Indented));

            trustDBService.Add(builder.Package);
            //trustDBService.DBContext.SaveChanges();

            var dbPackage = trustDBService.Packages.FirstOrDefaultAsync().Result;

            var compareResult = builder.Package.JsonCompare(dbPackage);
            Assert.IsTrue(compareResult, "Package from database is not the same as Builder");

            //Assert.AreEqual(2, trustDBService.DBContext.Claims.Count(), "Wrong number of Claims");
        }

        [TestMethod]
        public void Add2Claims2()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();

            var _trustBuilder = new TrustBuilder(ServiceProvider);
            _trustBuilder.SetServer("testserver");
            _trustBuilder.AddTrust("testissuer1", "testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .AddTrust("testissuer2", "testsubject2", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build().Sign();

            Console.WriteLine(JsonConvert.SerializeObject(_trustBuilder.Package, Formatting.Indented));

            trustDBService.Add(_trustBuilder.Package);
            //trustDBService.DBContext.SaveChanges();

            var dbPackage = trustDBService.Packages.FirstOrDefaultAsync().Result;

            var compareResult = _trustBuilder.Package.JsonCompare(dbPackage);
            Assert.IsTrue(compareResult, "Package from database is not the same as Builder");

            //Assert.AreEqual(2, trustDBService.DBContext.Claims.Count(), "Wrong number of Claims");
        }

        [TestMethod]
        public void Add2Trusts()
        {
            var trustDBService = ServiceProvider.GetRequiredService<ITrustDBService>();
            var builder = new TrustBuilder(ServiceProvider);
            builder.SetServer("testserver");
            builder.AddTrust("testissuer1")
                .AddSubject("testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();

            var builder2 = new TrustBuilder(ServiceProvider);
            builder2.SetServer("testserver");
            builder2.AddTrust("testissuer1")
                .AddSubject("testsubject1", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .AddTrust("testissuer2")
                .AddSubject("testsubject2", TrustBuilder.BINARY_TRUST_DTP1, TrustBuilder.CreateBinaryTrustAttributes(true))
                .Build()
                .Sign();


            Console.WriteLine(JsonConvert.SerializeObject(builder.Package, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(builder2.Package, Formatting.Indented));

            trustDBService.Add(builder.Package);
            trustDBService.Add(builder2.Package);

            var dbPackage = trustDBService.Packages.OrderBy(p=>p.DatabaseID).FirstOrDefaultAsync().Result;

            var compareResult = builder.Package.JsonCompare(dbPackage);

            Assert.IsTrue(compareResult, "Package from database is not the same as Builder");

            Assert.AreEqual(3, trustDBService.DBContext.Trusts.Count(), "Wrong number of Trusts");
        }


    }
}

