using DtpCore.Interfaces;
using DtpCore.Model;
using DtpPackageCore.Commands;
using DtpPackageCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTest.DtpPackage.Mocks;

namespace UnitTest.DtpPackage
{
    [TestClass]
    public class TrustPackageCommandHandlerTest : StartupMock
    {

        //[TestMethod]
        //[ExpectedException(typeof(AggregateException))]
        //public void Create()
        //{
        //    var name = "Test";
        //    var package = new Package();

        //    var result = Mediator.Send(new CreateTrustPackageCommand { Name = name, TrustPackage = package }).Result;
        //    Console.WriteLine("Done!");
        //}

        //[TestMethod]
        //public void Update()
        //{
        //    var name = "Test";
        //    var package = new Package();

        //    var result = Mediator.Send(new UpdateTrustPackageCommand { Name = name, TrustPackage = package }).Result;
        //    Assert.IsTrue(result);
        //}

    }
}
